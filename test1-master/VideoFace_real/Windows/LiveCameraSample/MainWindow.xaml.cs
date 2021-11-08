// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using VideoFrameAnalyzer;
using Common = Microsoft.ProjectOxford.Common;
using FaceAPI = Microsoft.ProjectOxford.Face;
using VisionAPI = Microsoft.ProjectOxford.Vision;


using ClientContract = Microsoft.ProjectOxford.Face.Contract;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Window = System.Windows.Window;
using Microsoft.ProjectOxford.Face;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;
using System.Data.SqlClient;

namespace LiveCameraSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        private FaceAPI.FaceServiceClient _faceClient = null;
        private VisionAPI.VisionServiceClient _visionClient = null;
        private readonly FrameGrabber<LiveCameraResult> _grabber = null;
        private static readonly ImageEncodingParam[] s_jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };
        private readonly CascadeClassifier _localFaceDetector = new CascadeClassifier();
        private bool _fuseClientRemoteResults;
        private LiveCameraResult _latestResultsToDisplay = null;
        private AppMode _mode;
        private DateTime _startTime;
        private int _timeInverval = 10;

        public enum AppMode
        {
            FacesIdntifyWithClient,
            Faces,
            Emotions,
            EmotionsWithClientFaceDetect,
            Tags,
            Celebrities
        }

        public MainWindow()
        {
            InitializeComponent();

            // Create grabber. 
            _grabber = new FrameGrabber<LiveCameraResult>();

            // Set up a listener for when the client receives a new frame.
            _grabber.NewFrameProvided += (s, e) =>
            {
                if (_mode == AppMode.EmotionsWithClientFaceDetect)
                {
                    // Local face detection. 
                    var rects = _localFaceDetector.DetectMultiScale(e.Frame.Image);
                    // Attach faces to frame. 
                    e.Frame.UserData = rects;
                }

                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    // Display the image in the left pane.
                    LeftImage.Source = e.Frame.Image.ToBitmapSource();

                    // If we're fusing client-side face detection with remote analysis, show the
                    // new frame now with the most recent analysis available. 
                    if (_fuseClientRemoteResults)
                    {
                        RightImage.Source = VisualizeResult(e.Frame);
                    }
                }));

                // See if auto-stop should be triggered. 
                if (Properties.Settings.Default.AutoStopEnabled && (DateTime.Now - _startTime) > Properties.Settings.Default.AutoStopTime)
                {
                    _grabber.StopProcessingAsync();
                }


                _maxConcurrentProcesses = 10;
            };

            // Set up a listener for when the client receives a new result from an API call. 
            _grabber.NewResultAvailable += (s, e) =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.TimedOut)
                    {
                        MessageArea.Text = "API call timed out.";
                    }
                    else if (e.Exception != null)
                    {
                        string apiName = "";
                        string message = e.Exception.Message;
                        var faceEx = e.Exception as FaceAPI.FaceAPIException;
                        var emotionEx = e.Exception as Common.ClientException;
                        var visionEx = e.Exception as VisionAPI.ClientException;
                        if (faceEx != null)
                        {
                            apiName = "Face";
                            message = faceEx.ErrorMessage;
                        }
                        else if (emotionEx != null)
                        {
                            apiName = "Emotion";
                            message = emotionEx.Error.Message;
                        }
                        else if (visionEx != null)
                        {
                            apiName = "Computer Vision";
                            message = visionEx.Error.Message;
                        }
                        MessageArea.Text = string.Format("{0} API call failed on frame {1}. Exception: {2}", apiName, e.Frame.Metadata.Index, message);
                    }
                    else
                    {
                        _latestResultsToDisplay = e.Analysis;

                        // Display the image and visualization in the right pane. 
                        if (!_fuseClientRemoteResults)
                        {
                            RightImage.Source = VisualizeResult(e.Frame);
                        }
                    }
                }));
            };

            // Create local face detector. 
            _localFaceDetector.Load("Data/haarcascade_frontalface_alt2.xml");
        }

        /// <summary> Function which submits a frame to the Face API. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the faces returned by the API. </returns>
        private async Task<LiveCameraResult> FacesIdentifyAnalysisFunctionWithClient(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            // Submit image to API.          
            var faces = await _faceClient.DetectAsync(jpg);

            // Identify each face
            // Call identify REST API, the result contains identified person information
            var identifyResult = await _faceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: this.GroupId);

            // Count the API call. 
            Properties.Settings.Default.FaceAPICallCount++;

            // Output. 
            return new LiveCameraResult { Faces = faces, IdentifyResults = identifyResult };
        }

        /// <summary> Function which submits a frame to the Face API. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the faces returned by the API. </returns>
        private async Task<LiveCameraResult> FacesAnalysisFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            // Submit image to API. 
            var attrs = new List<FaceAPI.FaceAttributeType> {
                FaceAPI.FaceAttributeType.Age,
                FaceAPI.FaceAttributeType.Gender,
                FaceAPI.FaceAttributeType.HeadPose
            };
            var faces = await _faceClient.DetectAsync(jpg, returnFaceAttributes: attrs);
            // Count the API call. 
            Properties.Settings.Default.FaceAPICallCount++;
            // Output. 
            return new LiveCameraResult { Faces = faces };
        }

        /// <summary> Function which submits a frame to the Emotion API. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the emotions returned by the API. </returns>
        private async Task<LiveCameraResult> EmotionAnalysisFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            // Submit image to API. 
            FaceAPI.Contract.Face[] faces = null;

            // See if we have local face detections for this image.
            var localFaces = (OpenCvSharp.Rect[])frame.UserData;
            if (localFaces == null || localFaces.Count() > 0)
            {
                // If localFaces is null, we're not performing local face detection.
                // Use Cognigitve Services to do the face detection.
                Properties.Settings.Default.FaceAPICallCount++;
                faces = await _faceClient.DetectAsync(
                    jpg,
                    /* returnFaceId= */ false,
                    /* returnFaceLandmarks= */ false,
                    new FaceAPI.FaceAttributeType[1] { FaceAPI.FaceAttributeType.Emotion });
            }
            else
            {
                // Local face detection found no faces; don't call Cognitive Services.
                faces = new FaceAPI.Contract.Face[0];
            }

            // Output. 
            return new LiveCameraResult
            {
                Faces = faces.Select(e => CreateFace(e.FaceRectangle)).ToArray(),
                // Extract emotion scores from results. 
                EmotionScores = faces.Select(e => e.FaceAttributes.Emotion).ToArray()
            };
        }

        /// <summary> Function which submits a frame to the Computer Vision API for tagging. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the tags returned by the API. </returns>
        private async Task<LiveCameraResult> TaggingAnalysisFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            // Submit image to API. 
            var analysis = await _visionClient.GetTagsAsync(jpg);
            // Count the API call. 
            Properties.Settings.Default.VisionAPICallCount++;
            // Output. 
            return new LiveCameraResult { Tags = analysis.Tags };
        }

        /// <summary> Function which submits a frame to the Computer Vision API for celebrity
        ///     detection. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the celebrities returned by the API. </returns>
        private async Task<LiveCameraResult> CelebrityAnalysisFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            // Submit image to API. 
            var result = await _visionClient.AnalyzeImageInDomainAsync(jpg, "celebrities");
            // Count the API call. 
            Properties.Settings.Default.VisionAPICallCount++;
            // Output. 
            var celebs = JsonConvert.DeserializeObject<CelebritiesResult>(result.Result.ToString()).Celebrities;
            return new LiveCameraResult
            {
                // Extract face rectangles from results. 
                Faces = celebs.Select(c => CreateFace(c.FaceRectangle)).ToArray(),
                // Extract celebrity names from results. 
                CelebrityNames = celebs.Select(c => c.Name).ToArray()
            };
        }

        ///////////////////////////
        private void DB_output()
        {

            string subid = "1000";
            string connString = @"Data Source=hanium.database.windows.net;Initial Catalog=Face;Persist Security Info=True;User ID=FaceAdmin;Password=FaceAPI@2019";

            SqlConnection conn = new SqlConnection(connString);

            SqlCommand cmd = new SqlCommand("SELECT student_number FROM registration WHERE subject_id=" + subid + " order by student_number asc");
            cmd.Connection = conn;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                AttendeeList.Items.Insert(0, reader["student_number"].ToString());
            }
            reader.Close();
            conn.Close();
        }   
        private void DB_getsubid()
        {
            string connString = @"Data Source=hanium.database.windows.net;Initial Catalog=Face;Persist Security Info=True;User ID=FaceAdmin;Password=FaceAPI@2019";

            SqlConnection conn = new SqlConnection(connString);

            SqlCommand cmd = new SqlCommand("SELECT sub_id FROM subject ");
            cmd.Connection = conn;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            String sub_id = reader["sub_id"].ToString();
            reader.Close();
            conn.Close();
        }
        ///////////////////////////////


        private BitmapSource VisualizeResult(VideoFrame frame)
        {
            // Draw any results on top of the image. 
            BitmapSource visImage = frame.Image.ToBitmapSource();

            var result = _latestResultsToDisplay;


            if (result != null)
            {
                // See if we have local face detections for this image.
                var clientFaces = (OpenCvSharp.Rect[])frame.UserData;
                if (clientFaces != null && result.Faces != null)
                {
                    // If so, then the analysis results might be from an older frame. We need to match
                    // the client-side face detections (computed on this frame) with the analysis
                    // results (computed on the older frame) that we want to display. 
                    MatchAndReplaceFaceRectangles(result.Faces, clientFaces);

                }

                //얼굴 두개 이상
                result.PersonNames = new string[result.IdentifyResults.Length];

                if (result.IdentifyResults != null && result.IdentifyResults.Length > 0)
                {
                    for (int idx = 0; idx < result.IdentifyResults.Length; idx++)
                    {
                        // Update identification result for rendering
                        var face = result.Faces[idx];
                        var res = result.IdentifyResults[idx];
                        
                        if (res.Candidates.Length > 0 && Persons.Any(p => p.PersonId == res.Candidates[0].PersonId.ToString()))
                        {
                            if (result.Faces[idx].FaceId == result.IdentifyResults[idx].FaceId)
                            {

                                result.PersonNames[idx] = Persons.Where(p => p.PersonId == res.Candidates[0].PersonId.ToString()).First().PersonName;
                            }
                            else
                            {
                                result.PersonNames[idx] = "Unknown";
                            }
                        }
                        else
                        {
                            result.PersonNames[idx] = "Unknown";
                        }
                    }
                }
                

                visImage = Visualization.DrawFaces(visImage, result.Faces, result.EmotionScores, result.CelebrityNames, result.PersonNames);
                visImage = Visualization.DrawTags(visImage, result.Tags);

                //DB Operation
                if(result.PersonNames !=null)
                    DB_Operation(result.PersonNames);
            }

            return visImage;
        }

        private async void DB_Operation(string[] personName)
        {
            string subid = "1000";
            string attendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string startTime = _startTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string endTime = _startTime.AddMinutes(_timeInverval).ToString("yyyy-MM-dd HH:mm:ss.fff");
            string connString = @"Data Source=hanium.database.windows.net;Initial Catalog=Face;Persist Security Info=True;User ID=FaceAdmin;Password=FaceAPI@2019";
            string insertSQL = "";

            foreach (var item in personName)
            {
                if (item != null && item.ToLower() != "unknown")
                {

                    if (AttendeeList.Items.Contains(item))
                    {
                        AttendeeList.Items.Remove(item);

                        insertSQL = "  DECLARE @student_number INT; SELECT @student_number=s.student_number FROM Student t INNER JOIN Registration s ON t.student_number=s.student_number ";
                        insertSQL += " WHERE t.student_number='" + item + "' and s.subject_id=" + subid + ";";
                        insertSQL += " IF NOT EXISTS (SELECT * FROM Attendance WHERE subject_id=" + subid + "  AND student_number=@student_number AND Date BETWEEN  '" + startTime + "' AND  '" + endTime + "')";
                        insertSQL += " INSERT Attendance VALUES(" + subid + ",'" + attendTime + "', @student_number);";

                        using (SqlConnection conn = new SqlConnection(connString))
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                await conn.OpenAsync();
                                cmd.CommandText = insertSQL;
                                cmd.Connection = conn;
                                await cmd.ExecuteNonQueryAsync();
                            }
                        } // DB INSERT

                    } // !AttendeeList.Items.Contains(item)
                }

            }
        }
    
                /// <summary> Populate CameraList in the UI, once it is loaded. </summary>
                /// <param name="sender"> Source of the event. </param>
                /// <param name="e">      Routed event information. </param>
                private void CameraList_Loaded(object sender, RoutedEventArgs e)
                {
                    int numCameras = _grabber.GetNumCameras();

                    if (numCameras == 0)
                    {
                        MessageArea.Text = "No cameras found!";
                    }

                    var comboBox = sender as ComboBox;
                    comboBox.ItemsSource = Enumerable.Range(0, numCameras).Select(i => string.Format("Camera {0}", i + 1));
                    comboBox.SelectedIndex = 0;
                }

                /// <summary> Populate ModeList in the UI, once it is loaded. </summary>
                /// <param name="sender"> Source of the event. </param>
                /// <param name="e">      Routed event information. </param>
                private void ModeList_Loaded(object sender, RoutedEventArgs e)
                {
                    var modes = (AppMode[])Enum.GetValues(typeof(AppMode));

                    var comboBox = sender as ComboBox;
                    comboBox.ItemsSource = modes.Select(m => m.ToString());
                    comboBox.SelectedIndex = 0;
                }

                private void ModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    // Disable "most-recent" results display. 
                    _fuseClientRemoteResults = false;

                    var comboBox = sender as ComboBox;
                    var modes = (AppMode[])Enum.GetValues(typeof(AppMode));
                    _mode = modes[comboBox.SelectedIndex];
                    switch (_mode)
                    {
                        case AppMode.Faces:
                            _grabber.AnalysisFunction = FacesAnalysisFunction;
                            break;
                        case AppMode.FacesIdntifyWithClient:
                            _grabber.AnalysisFunction = FacesIdentifyAnalysisFunctionWithClient;
                            _fuseClientRemoteResults = true;
                            break;
                        case AppMode.Emotions:
                            _grabber.AnalysisFunction = EmotionAnalysisFunction;
                            break;
                        case AppMode.EmotionsWithClientFaceDetect:
                            // Same as Emotions, except we will display the most recent faces combined with
                            // the most recent API results. 
                            _grabber.AnalysisFunction = EmotionAnalysisFunction;
                            _fuseClientRemoteResults = true;
                            break;
                        case AppMode.Tags:
                            _grabber.AnalysisFunction = TaggingAnalysisFunction;
                            break;
                        case AppMode.Celebrities:
                            _grabber.AnalysisFunction = CelebrityAnalysisFunction;
                            break;
                        default:
                            _grabber.AnalysisFunction = null;
                            break;
                    }
                }

                private async void StartButton_Click(object sender, RoutedEventArgs e)
                {
                    if (!CameraList.HasItems)
                    {
                        MessageArea.Text = "No cameras found; cannot start processing";
                        return;
                    }
                    if (AttendeeList.Items.Count == 0)
                    {
                        DB_output();
                    }
                    // Clean leading/trailing spaces in API keys. 
                    Properties.Settings.Default.FaceAPIKey = Properties.Settings.Default.FaceAPIKey.Trim();
                    Properties.Settings.Default.VisionAPIKey = Properties.Settings.Default.VisionAPIKey.Trim();

                    // Create API clients. 
                    _faceClient = new FaceAPI.FaceServiceClient(Properties.Settings.Default.FaceAPIKey, Properties.Settings.Default.FaceAPIHost);
                    _visionClient = new VisionAPI.VisionServiceClient(Properties.Settings.Default.VisionAPIKey, Properties.Settings.Default.VisionAPIHost);

                    // How often to analyze. 
                    _grabber.TriggerAnalysisOnInterval(Properties.Settings.Default.AnalysisInterval);

                    // Reset message. 
                    MessageArea.Text = "";

                    // Record start time, for auto-stop
                    _startTime = DateTime.Now;

                    await _grabber.StartProcessingCameraAsync(CameraList.SelectedIndex);
                }

                private async void StopButton_Click(object sender, RoutedEventArgs e)
                {
                    await _grabber.StopProcessingAsync();
                }

                private void SettingsButton_Click(object sender, RoutedEventArgs e)
                {
                    SettingsPanel.Visibility = 1 - SettingsPanel.Visibility;
                }

                private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
                {
                    SettingsPanel.Visibility = Visibility.Hidden;
                    Properties.Settings.Default.Save();
                }

                private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                    e.Handled = true;
                }

                private FaceAPI.Contract.Face CreateFace(FaceAPI.Contract.FaceRectangle rect)
                {
                    return new FaceAPI.Contract.Face
                    {
                        FaceRectangle = new FaceAPI.Contract.FaceRectangle
                        {
                            Left = rect.Left,
                            Top = rect.Top,
                            Width = rect.Width,
                            Height = rect.Height
                        }
                    };
                }

                private FaceAPI.Contract.Face CreateFace(VisionAPI.Contract.FaceRectangle rect)
                {
                    return new FaceAPI.Contract.Face
                    {
                        FaceRectangle = new FaceAPI.Contract.FaceRectangle
                        {
                            Left = rect.Left,
                            Top = rect.Top,
                            Width = rect.Width,
                            Height = rect.Height
                        }
                    };
                }

                private FaceAPI.Contract.Face CreateFace(Common.Rectangle rect)
                {
                    return new FaceAPI.Contract.Face
                    {
                        FaceRectangle = new FaceAPI.Contract.FaceRectangle
                        {
                            Left = rect.Left,
                            Top = rect.Top,
                            Width = rect.Width,
                            Height = rect.Height
                        }
                    };
                }

        private void MatchAndReplaceFaceRectangles(FaceAPI.Contract.Face[] faces, OpenCvSharp.Rect[] clientRects)
        {
            // Use a simple heuristic for matching the client-side faces to the faces in the
            // results. Just sort both lists left-to-right, and assume a 1:1 correspondence. 

            // Sort the faces left-to-right. 
            var sortedResultFaces = faces
                .OrderBy(f => f.FaceRectangle.Left + 0.5 * f.FaceRectangle.Width)
                .ToArray();

            // Sort the clientRects left-to-right.
            var sortedClientRects = clientRects
                .OrderBy(r => r.Left + 0.5 * r.Width)
                .ToArray();

            // Assume that the sorted lists now corrrespond directly. We can simply update the
            // FaceRectangles in sortedResultFaces, because they refer to the same underlying
            // objects as the input "faces" array. 
            for (int i = 0; i < Math.Min(faces.Length, clientRects.Length); i++)
            {
                // convert from OpenCvSharp rectangles
                OpenCvSharp.Rect r = sortedClientRects[i];
                sortedResultFaces[i].FaceRectangle = new FaceAPI.Contract.FaceRectangle { Left = r.Left, Top = r.Top, Width = r.Width, Height = r.Height };
            }
        }


            
        // Video
            #region Fields

            /// <summary>
            /// Description dependency property
            /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(MainWindow));

        /// <summary>
        /// Temporary group id for create person database.
        /// </summary>
        private static string sampleGroupId = Guid.NewGuid().ToString();

        /// <summary>
        /// Faces to identify
        /// </summary>
        private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

        /// <summary>
        /// Person database
        /// </summary>
        private ObservableCollection<Person> _persons = new ObservableCollection<Person>();

        /// <summary>
        /// User picked image file
        /// </summary>
        private ImageSource _selectedFile;

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        #endregion Fields


       

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets description
        /// </summary>
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }

            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets group id.
        /// </summary>
        public string GroupId
        {
            get
            {
                return sampleGroupId;
            }

            set
            {
                sampleGroupId = value;
            }
        }

        /// <summary>
        /// Gets constant maximum image size for rendering detection result
        /// </summary>
        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// Gets person database
        /// </summary>
        public ObservableCollection<Person> Persons
        {
            get
            {
                return _persons;
            }
        }

        /// <summary>
        /// Gets or sets user picked image file
        /// </summary>
        public ImageSource SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        /// <summary>
        /// Gets faces to identify
        /// </summary>
        public ObservableCollection<Face> TargetFaces
        {
            get
            {
                return _faces;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Pick the root person database folder, to minimum the data preparation logic, the folder should be under following construction
        /// Each person's image should be put into one folder named as the person's name
        /// All person's image folder should be put directly under the root person database folder
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        private async void FolderPicker_Click(object sender, RoutedEventArgs e)
        {
            bool groupExists = false;

            MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
            string subscriptionKey = Properties.Settings.Default.FaceAPIKey.Trim();
            string endpoint = Properties.Settings.Default.FaceAPIHost.Trim();

            var faceServiceClient = new FaceAPI.FaceServiceClient(subscriptionKey, endpoint);

          

            // Test whether the group already exists
            try
            {
                MessageArea.Text =String.Format("Request: Group {0} will be used to build a person database. Checking whether the group exists.", this.GroupId);

                await faceServiceClient.GetLargePersonGroupAsync(this.GroupId);
                groupExists = true;
                MessageArea.Text = String.Format("Response: Group {0} exists.", this.GroupId);
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode != "LargePersonGroupNotFound")
                {
                    MessageArea.Text = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }
                else
                {
                    MessageArea.Text = String.Format("Response: Group {0} did not exist previously.", this.GroupId);
                }
            }

            if (groupExists)
            {
                var cleanGroup = System.Windows.MessageBox.Show(string.Format("Requires a clean up for group \"{0}\" before setting up a new person database. Click OK to proceed, group \"{0}\" will be cleared.", this.GroupId), "Warning", MessageBoxButton.OKCancel);
                if (cleanGroup == MessageBoxResult.OK)
                {
                    await faceServiceClient.DeleteLargePersonGroupAsync(this.GroupId);
                    this.GroupId = Guid.NewGuid().ToString();
                }
                else
                {
                    return;
                }
            }

            // Show folder picker
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();

            // Set the suggestion count is intent to minimum the data preparation step only,
            // it's not corresponding to service side constraint
            const int SuggestionCount = 15;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // User picked a root person database folder
                // Clear person database
                Persons.Clear();
                TargetFaces.Clear();
                SelectedFile = null;
                IdentifyButton.IsEnabled = false;

                // Call create large person group REST API
                // Create large person group API call will failed if group with the same name already exists
                MessageArea.Text = String.Format("Request: Creating group \"{0}\"", this.GroupId);
                try
                {
                    await faceServiceClient.CreateLargePersonGroupAsync(this.GroupId, this.GroupId);
                    MessageArea.Text = String.Format("Response: Success. Group \"{0}\" created", this.GroupId);
                }
                catch (FaceAPIException ex)
                {
                    MessageArea.Text = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    return;
                }

                int processCount = 0;
                bool forceContinue = false;

                MessageArea.Text = String.Format("Request: Preparing faces for identification, detecting faces in chosen folder.");

                // Enumerate top level directories, each directory contains one person's images
                int invalidImageCount = 0;
                foreach (var dir in System.IO.Directory.EnumerateDirectories(dlg.SelectedPath))
                {
                    var tasks = new List<Task>();
                    var tag = System.IO.Path.GetFileName(dir);
                    Person p = new Person();
                    p.PersonName = tag;

                    var faces = new ObservableCollection<Face>();
                    p.Faces = faces;

                    // Call create person REST API, the new create person id will be returned
                    MessageArea.Text = String.Format("Request: Creating person \"{0}\"", p.PersonName);
                    p.PersonId = (await faceServiceClient.CreatePersonInLargePersonGroupAsync(this.GroupId, p.PersonName)).PersonId.ToString();
                    MessageArea.Text = String.Format("Response: Success. Person \"{0}\" (PersonID:{1}) created", p.PersonName, p.PersonId);

                    string img;
                    // Enumerate images under the person folder, call detection
                    var imageList =
                    new ConcurrentBag<string>(
                        Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                            .Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".bmp") || s.ToLower().EndsWith(".gif")));

                    while (imageList.TryTake(out img))
                    {
                        tasks.Add(Task.Factory.StartNew(
                            async (obj) =>
                            {
                                var imgPath = obj as string;

                                using (var fStream = File.OpenRead(imgPath))
                                {
                                    try
                                    {
                                        // Update person faces on server side
                                        var persistFace = await faceServiceClient.AddPersonFaceInLargePersonGroupAsync(this.GroupId, Guid.Parse(p.PersonId), fStream, imgPath);
                                        return new Tuple<string, ClientContract.AddPersistedFaceResult>(imgPath, persistFace);
                                    }
                                    catch (FaceAPIException ex)
                                    {
                                        // if operation conflict, retry.
                                        if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                                        {
                                            imageList.Add(imgPath);
                                            return null;
                                        }
                                        // if operation cause rate limit exceed, retry.
                                        else if (ex.ErrorCode.Equals("RateLimitExceeded"))
                                        {
                                            imageList.Add(imgPath);
                                            return null;
                                        }
                                        else if (ex.ErrorMessage.Contains("more than 1 face in the image."))
                                        {
                                            Interlocked.Increment(ref invalidImageCount);
                                        }
                                        // Here we simply ignore all detection failure in this sample
                                        // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                                        return new Tuple<string, ClientContract.AddPersistedFaceResult>(imgPath, null);
                                    }
                                }
                            },
                            img).Unwrap().ContinueWith((detectTask) =>
                            {
                                // Update detected faces for rendering
                                var detectionResult = detectTask?.Result;
                                if (detectionResult == null || detectionResult.Item2 == null)
                                {
                                    return;
                                }

                                this.Dispatcher.Invoke(
                                    new Action<ObservableCollection<Face>, string, ClientContract.AddPersistedFaceResult>(UIHelper.UpdateFace),
                                    faces,
                                    detectionResult.Item1,
                                    detectionResult.Item2);
                            }));
                        if (processCount >= SuggestionCount && !forceContinue)
                        {
                            var continueProcess = System.Windows.Forms.MessageBox.Show("The images loaded have reached the recommended count, may take long time if proceed. Would you like to continue to load images?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (continueProcess == System.Windows.Forms.DialogResult.Yes)
                            {
                                forceContinue = true;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (tasks.Count >= _maxConcurrentProcesses || imageList.IsEmpty)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }

                    Persons.Add(p);
                }
                if (invalidImageCount > 0)
                {
                   MessageArea.Text=String.Format("Warning: more or less than one face is detected in {0} images, can not add to face list.", invalidImageCount);
                }
                MessageArea.Text = String.Format("Response: Success. Total {0} faces are detected.", Persons.Sum(p => p.Faces.Count));

                try
                {
                    // Start train large person group
                    MessageArea.Text = String.Format("Request: Training group \"{0}\"", this.GroupId);
                    await faceServiceClient.TrainLargePersonGroupAsync(this.GroupId);

                    // Wait until train completed
                    while (true)
                    {
                        await Task.Delay(1000);
                        var status = await faceServiceClient.GetLargePersonGroupTrainingStatusAsync(this.GroupId);
                        MessageArea.Text = String.Format("Response: {0}. Group \"{1}\" training process is {2}", "Success", this.GroupId, status.Status);
                        if (status.Status != FaceAPI.Contract.Status.Running)
                        {
                            break;
                        }
                    }
                    IdentifyButton.IsEnabled = true;
                }
                catch (FaceAPIException ex)
                {
                    MessageArea.Text = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                }
            }
            GC.Collect();


            PersonGroupTraining.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Pick image, detect and identify all faces detected
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private async void Identify_Click(object sender, RoutedEventArgs e)
        {
            // Show file picker
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files(*.jpg, *.png, *.bmp, *.gif) | *.jpg; *.png; *.bmp; *.gif";
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                // User picked one image
                // Clear previous detection and identification results
                TargetFaces.Clear();
                var pickedImagePath = dlg.FileName;
                var renderingImage = UIHelper.LoadImageAppliedOrientation(pickedImagePath);
                var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                SelectedFile = renderingImage;

                var sw = Stopwatch.StartNew();

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                string subscriptionKey = Properties.Settings.Default.FaceAPIKey.Trim();
                string endpoint = Properties.Settings.Default.FaceAPIHost.Trim();
                var faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

                // Call detection REST API
                using (var fStream = File.OpenRead(pickedImagePath))
                {
                    try
                    {
                        var faces = await faceServiceClient.DetectAsync(fStream);

                        // Convert detection result into UI binding object for rendering
                        foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                        {
                            TargetFaces.Add(face);
                        }

                        MessageArea.Text = String.Format("Request: Identifying {0} face(s) in group \"{1}\"", faces.Length, this.GroupId);

                        // Identify each face
                        // Call identify REST API, the result contains identified person information
                        var identifyResult = await faceServiceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: this.GroupId);
                        for (int idx = 0; idx < faces.Length; idx++)
                        {
                            // Update identification result for rendering
                            var face = TargetFaces[idx];
                            var res = identifyResult[idx];
                            if (res.Candidates.Length > 0 && Persons.Any(p => p.PersonId == res.Candidates[0].PersonId.ToString()))
                            {
                                face.PersonName = Persons.Where(p => p.PersonId == res.Candidates[0].PersonId.ToString()).First().PersonName;
                            }
                            else
                            {
                                face.PersonName = "Unknown";
                            }
                        }

                        var outString = new StringBuilder();
                        foreach (var face in TargetFaces)
                        {
                            outString.AppendFormat("Face {0} is identified as {1}. ", face.FaceId, face.PersonName);
                        }

                        MessageArea.Text = String.Format("Response: Success. {0}", outString);
                    }
                    catch (FaceAPIException ex)
                    {
                        MessageArea.Text = String.Format("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    }
                }
            }
            GC.Collect();
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        /// Identification result for UI binding
        /// </summary>
        public class IdentificationResult : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Face to identify
            /// </summary>
            private Face _faceToIdentify;

            /// <summary>
            /// Identified person's name
            /// </summary>
            private string _name;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets face to identify
            /// </summary>
            public Face FaceToIdentify
            {
                get
                {
                    return _faceToIdentify;
                }

                set
                {
                    _faceToIdentify = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FaceToIdentify"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets identified person's name
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }

                set
                {
                    _name = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));
                    }
                }
            }

            #endregion Properties
        }

        /// <summary>
        /// Person structure for UI binding
        /// </summary>
        public class Person : INotifyPropertyChanged
        {
            #region Fields

            /// <summary>
            /// Person's faces from database
            /// </summary>
            private ObservableCollection<Face> _faces = new ObservableCollection<Face>();

            /// <summary>
            /// Person's id
            /// </summary>
            private string _personId;

            /// <summary>
            /// Person's name
            /// </summary>
            private string _personName;

            #endregion Fields

            #region Events

            /// <summary>
            /// Implement INotifyPropertyChanged interface
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets or sets person's faces from database
            /// </summary>
            public ObservableCollection<Face> Faces
            {
                get
                {
                    return _faces;
                }

                set
                {
                    _faces = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Faces"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's id
            /// </summary>
            public string PersonId
            {
                get
                {
                    return _personId;
                }

                set
                {
                    _personId = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonId"));
                    }
                }
            }

            /// <summary>
            /// Gets or sets person's name
            /// </summary>
            public string PersonName
            {
                get
                {
                    return _personName;
                }

                set
                {
                    _personName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PersonName"));
                    }
                }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}
