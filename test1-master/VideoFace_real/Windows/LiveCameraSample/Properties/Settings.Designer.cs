﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LiveCameraSample.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.2.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string FaceAPIKey {
            get {
                return ((string)(this["FaceAPIKey"]));
            }
            set {
                this["FaceAPIKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string VisionAPIKey {
            get {
                return ((string)(this["VisionAPIKey"]));
            }
            set {
                this["VisionAPIKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Visible")]
        public global::System.Windows.Visibility SettingsPanelVisibility {
            get {
                return ((global::System.Windows.Visibility)(this["SettingsPanelVisibility"]));
            }
            set {
                this["SettingsPanelVisibility"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:03")]
        public global::System.TimeSpan AnalysisInterval {
            get {
                return ((global::System.TimeSpan)(this["AnalysisInterval"]));
            }
            set {
                this["AnalysisInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int FaceAPICallCount {
            get {
                return ((int)(this["FaceAPICallCount"]));
            }
            set {
                this["FaceAPICallCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int VisionAPICallCount {
            get {
                return ((int)(this["VisionAPICallCount"]));
            }
            set {
                this["VisionAPICallCount"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoStopEnabled {
            get {
                return ((bool)(this["AutoStopEnabled"]));
            }
            set {
                this["AutoStopEnabled"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:05:00")]
        public global::System.TimeSpan AutoStopTime {
            get {
                return ((global::System.TimeSpan)(this["AutoStopTime"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://westus.api.cognitive.microsoft.com/face/v1.0")]
        public string FaceAPIHost {
            get {
                return ((string)(this["FaceAPIHost"]));
            }
            set {
                this["FaceAPIHost"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://westus.api.cognitive.microsoft.com/vision/v1.0")]
        public string VisionAPIHost {
            get {
                return ((string)(this["VisionAPIHost"]));
            }
            set {
                this["VisionAPIHost"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=hanium.database.windows.net;Initial Catalog=Face;Persist Security Inf" +
            "o=True;User ID=FaceAdmin;Password=FaceAPI@2019")]
        public string FaceConnectionString {
            get {
                return ((string)(this["FaceConnectionString"]));
            }
        }
    }
}
