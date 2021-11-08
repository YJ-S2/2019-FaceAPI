<%@ page language="java" contentType="text/html; charset=UTF-8"
   pageEncoding="UTF-8"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<%@ page import="java.sql.*"%>






<head>

<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<meta http-equiv="X-UA-Compatible" content="ie=edge">
<link rel="icon" type="image/png" href="img/logo1.png" />
<title>Student</title>
<link rel="stylesheet"
   href="https://fonts.googleapis.com/css?family=Roboto:400,700">
<!-- https://fonts.google.com/specimen/Roboto -->
<link rel="stylesheet" href="css/fontawesome.min.css">
<!-- https://fontawesome.com/ -->
<link rel="stylesheet" href="css/bootstrap.min.css">
<!-- https://getbootstrap.com/ -->


<link rel="stylesheet" href="css/templatemo-style.css">
<!--
   Product Admin CSS Template
   https://templatemo.com/tm-524-product-admin
   
   -->

</head>

<body id="reportsPage">
   <%
      String url = "jdbc:sqlserver://hanium.database.windows.net:1433;databaseName=Face;";

      String user = "FaceAdmin@hanium";

      String password = "FaceAPI@2019";

      //   String[] subject_name = new String[8];
      String[] date = new String[10];

      Connection conn = null;
      Statement stmt = null;
      ResultSet rs = null;
      ResultSet rs1 = null;
      try {
         Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");

         conn = DriverManager.getConnection(url, user, password);

         stmt = conn.createStatement();

         int idx = 0;

         //   ResultSet rs = stmt.executeQuery("SELECT subject_name FROM subject");
   %>
   <div class="" id="home">
      <nav class="navbar navbar-expand-xl">
      <div class="container h-100">
         <a class="navbar-brand" href="index.html">
            <h1 class="tm-site-title mb-0"></h1>
         </a>
         <button class="navbar-toggler ml-auto mr-0" type="button"
            data-toggle="collapse" data-target="#navbarSupportedContent"
            aria-controls="navbarSupportedContent" aria-expanded="false"
            aria-label="Toggle navigation">
            <i class="fas fa-bars tm-nav-icon"></i>
         </button>

         <div class="collapse navbar-collapse" id="navbarSupportedContent">
            <ul class="navbar-nav mx-auto h-100">
               <li class="nav-item"><a class="nav-link active" href="#">
                     <i class="fas fa-tachometer-alt"></i> 과목별 출석현황<span
                     class="sr-only">(current)</span>
               </a></li>

               <li class="nav-item"><a class="nav-link" href="accounts.html">
                     <i class="far fa-user"></i> 내 정보
               </a></li>

            </ul>
            <ul class="navbar-nav">
               <li class="nav-item"><a class="nav-link d-block"
                  href="login.html"> Student, <b>Logout</b>
               </a></li>
            </ul>
         </div>
      </div>
      </nav>




      <div class="container">
         <div class="tm-bg-primary-dark tm-block tm-block-h-auto">
            <h2 class="tm-block-title">수강 과목</h2>
            <!--    <p class="text-white">Accounts</p> -->
            <select class="custom-select">
               <%
                  String query = "SELECT subject_name FROM subject";

                     rs = stmt.executeQuery(query);
                     while (rs.next()) {
                        String sub_name = rs.getString("subject_name");
               %>
               <option value="0">

                  <%=sub_name%>


               </option>
               <%
                  }
                     rs.close();

                     stmt.close();

                     conn.close();


               %>

            </select>
         </div>
         &nbsp; &nbsp; &nbsp;



         <!-- row -->



         <div class="tm-bg-primary-dark tm-block tm-block-h-auto">
            <div class="col-12 tm-block-col">
               <div class="tm-bg-primary-dark tm-block tm-block-h-auto ">
                  <h2 class="tm-block-title">출석 현황</h2>


                  <table class="table">
                     <thead>
                        <tr>
                           <th scope="col">WEEK</th>
                           <th scope="col">출석  </th>
                           <th scope="col">TIME</th>
                     <th scope="col">TIME</th>

                        </tr>
                     </thead>
                     <tbody>
                        <tr>
                           <th scope="row"><b>WEEK 1</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>

                           <td>
                              2019-07-31 17:57:00 
                           </td>
                           <td>
                               2019-07-31 19:00:00 
                           </td>






                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 2</b></th>
                           <td>
                              <div class="tm-status-circle pending"></div>
                           </td>
                           <td>
                              
                           </td>
                           <td>
                               2019-08-01 19:00:00 
                           </td>


                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 3</b></th>
                           <td>
                              <div class="tm-status-circle cancelled"></div>
                           </td>
                           <td>
                              
                           </td>
                           <td>
                            
                           </td>


                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 4</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>
                           <td>
                               2019-08-03 17:57:00 
                           </td>
                           <td>
                                2019-08-03 19:00:00 
                           </td>


                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 5</b></th>
                           <td>
                              <div class="tm-status-circle pending"></div>
                           </td>

                           <td>
                              
                           </td>
                           <td>
                               2019-08-04 19:00:00 
                           </td>



                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 6</b></th>
                           <td>
                              <div class="tm-status-circle pending"></div>
                           </td>
                           <td>
                              
                           </td>
                           <td>
                              2019-08-05 19:00:00
                           </td>



                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 7</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>

                           <td>
                             2019-08-06 17:57:00 
                           </td>

                           <td>
                               2019-08-06 19:00:00
                           </td>




                        </tr>
                        <tr>
                           <th scope="row"><b>WEEK 8</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>
                           <td>
                               2019-08-07 17:57:00 
                           </td>
                           <td>
                               2019-08-07 19:00:00 
                           </td>
                        </tr>


                        <tr>
                           <th scope="row"><b>WEEK 9</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>

                           <td>
                             2019-08-08 17:57:00 
                           </td>
                           <td>
                               2019-08-08 19:00:00 
                           </td>



                        </tr>

                        <tr>
                           <th scope="row"><b>WEEK 10</b></th>
                           <td>
                              <div class="tm-status-circle moving"></div>
                           </td>
                           <td>
                               2019-08-10 17:57:00 
                           </td>
                           <td>
                               2019-08-10 19:00:00 
                             
                           </td>

                        </tr>
                     </tbody>

                  </table>
               </div>
            </div>
         </div>

      </div>

      <%
         } catch (SQLException sqle) {

            System.out.println("SQLException : " + sqle);

         }
      %>






      &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
      <!--    <center>
               <div class="tm-bg-primary-dark tm-block tm-block-h-auto">
                     <div class="tm-bg-primary-dark tm-block tm-block-taller">
                        <h2 class="tm-block-title">ì¶ ì   í  í © ê·¸ë  í  </h2>
                        <div id="pieChartContainer">
                           <canvas id="pieChart" class="chartjs-render-monitor"
                              width="2000" height="800"></canvas>
                        </div>
                     </div>
                  </div>
                  </center> -->

   </div>

   <script src="js/jquery-3.3.1.min.js"></script>
   <!-- https://jquery.com/download/ -->
   <script src="js/moment.min.js"></script>
   <!-- https://momentjs.com/ -->
   <script src="js/Chart.min.js"></script>
   <!-- http://www.chartjs.org/docs/latest/ -->
   <script src="js/bootstrap.min.js"></script>
   <!-- https://getbootstrap.com/ -->
   <script src="js/tooplate-scripts.js"></script>
   <script>
      Chart.defaults.global.defaultFontColor = 'white';
      let //ctxLine, ctxBar, 
      ctxPie, // optionsLine, optionsBar,
      optionsPie, //configLine, configBar, 
      configPie, //lineChart; barChart, 
      pieChart;
      // DOM is ready
      $(function() {
         //   drawLineChart(); // Line Chart
         //   drawBarChart(); // Bar Chart
         drawPieChart(); // Pie Chart

         $(window).resize(function() {
            updateLineChart();
            updateBarChart();
         });
      })
   </script>
</body>



</html>