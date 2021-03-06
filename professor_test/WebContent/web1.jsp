<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
    <title>Product Admin - Dashboard HTML Template</title>
    <link rel="icon" type="image/png" href="img/logo1.png"/>
    
    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:400,700">
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
    
    <!-- piechart js파일에서 가져옴 -->
    <script type="text/javascript" src="./js/tooplate-scripts.js" ></script> 
    
    <%@ page import="java.sql.*" %>
    
    <%
   
    String url = "jdbc:sqlserver://hanium.database.windows.net:1433;databaseName=Face;";
    String user = "FaceAdmin@hanium";
    String password = "FaceAPI@2019";
    
    String[] subject_name 	= new String[8]; 
    int[] student_number 	= new int[30];
    String[] name 			= new String[30];
    String[] department 	= new String[30];
   
    
    try {
    	
       Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
       
       Connection conn = DriverManager.getConnection(url, user, password);
       Statement stmt = conn.createStatement();
       
       ResultSet rsubject = stmt.executeQuery("SELECT subject_name FROM subject");
       
       int sub_idx = 0;
       while (rsubject.next()) {
          subject_name[sub_idx++] = rsubject.getString("subject_name");
       }
       
       rsubject.close();
       
       
       ResultSet rstudent = stmt.executeQuery("SELECT student_number, name, department FROM student");
       
       int stu_idx = 0;
       while (rstudent.next()) {
           student_number[stu_idx] = rstudent.getInt("student_number");
           name[stu_idx] = rstudent.getString("name");
           department[stu_idx] = rstudent.getString("department");
           stu_idx++;
        }
       
       rstudent.close();
       
       stmt.close();
       conn.close();
    } catch (SQLException sqle) {
       System.out.println("SQLException : " + sqle);
    }
    %>
    

	<style>
	.custom-select30 {
	  width: 70%;
	  border: none;
	  color: #acc6de;
	  height: 30px;
	  -webkit-appearance: none;
	  -moz-appearance: none;
	  -ms-appearance: none;
	  -o-appearance: none;
	  appearance: none;
	  -webkit-border-radius: 0;
	  -moz-border-radius: 0;
	  -ms-border-radius: 0;
	  -o-border-radius: 0;
	  border-radius: 0;
	  padding: 3px;
	  background: url(../img/arrow-down.png) 98% no-repeat #50657b;
	}
	.left-mj{
		 margin: 0 0 0 auto; 
	}
	.c_red{
		color:red;
	}
	.c_green{
		color:lightgreen;
	}
	.logo {
  width: 25%;
  height: 100%;
  display: -webkit-box;
  display: -webkit-flex;
  display: -moz-box;
  display: -ms-flexbox;
  display: flex;
  justify-content: flex-start;
  align-items: center;
}

.logo img {
  max-width: 100%;
  max-height: 50%;
}
.logo{display: none;}
	.percen{
		align:center;
		padding:20px;
	}
	.tm-status-circle1 {
  display: inline-block;
  margin-right: 5px;
  vertical-align: middle;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  margin-top: -3px;
}
	</style>
</head>

<body id="reportsPage">
    <div class="" id="home">
        <nav class="navbar navbar-expand-xl">
            <div class="container h-100">
                <a class="navbar-brand" href="index.html">
                	<h1 class="tm-site-title mb-0">FACE API</h1>
                	<!-- <img src="img/logo_2.png" alt="" class="">-->
                </a>
                <button class="navbar-toggler ml-auto mr-0" type="button" data-toggle="collapse" data-target="#navbarSupportedContent"
                    aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                    <i class="fas fa-bars tm-nav-icon"></i>
                </button>

                <div class="collapse navbar-collapse" id="navbarSupportedContent">
                    <ul class="navbar-nav mx-auto h-100">
                        <li class="nav-item">
                            <a class="nav-link active" href="#">
                                <i class="fas fa-tachometer-alt"></i>
                                Dashboard
                                <span class="sr-only">(current)</span>
                            </a>
                        </li>

                        <li class="nav-item">
                            <a class="nav-link" href="accounts.jsp">
                                <i class="far fa-user"></i>
                                Accounts
                            </a>
                        </li>
                    </ul>
                    <ul class="navbar-nav">
                        <li class="nav-item">
                            <a class="nav-link d-block" href="login.jsp">
                                Admin, <b>Logout</b>
                            </a>
                        </li>
                    </ul>
                </div>
            </div>

        </nav>
        
        
        <div class="container mt-5">
            <div class="row tm-content-row">
		          <div class="col-12 tm-block-col">
		            <div class="tm-bg-primary-dark tm-block tm-block-h-auto">
                      <h2 class="tm-block-title">강의 목록</h2>         
		              <select class="custom-select" id="ctg" onchange="CngList(i);">
		              <%
		              	for(int i = 0; i < subject_name.length; i++) { 
		              	%>
		              		<option value="0"><%=subject_name[i] %></option>
		              	<%
		              	}
		              %>
		              </select>
		            </div>
		          </div>
		        </div>
            <!-- row -->
            <div class="row tm-content-row">

                
                
                <div class="col-sm-12 col-md-12 col-lg-6 col-xl-6 tm-block-col">
                    <div class="tm-bg-primary-dark tm-block tm-block-taller">
                        <h2 class="tm-block-title">출석 현황 그래프</h2>
                        <div id="pieChartContainer">
                            <canvas id="pieChart" class="chartjs-render-monitor" width="200" height="200"></canvas>
                        </div>                        
                    </div>
                </div>
                
                
                
                
                <div class="col-sm-12 col-md-12 col-lg-6 col-xl-6 tm-block-col">
                    <div class="tm-bg-primary-dark tm-block tm-block-taller tm-block-overflow">
                        <h2 class="tm-block-title">수강 학생</h2>
                        <div class="tm-notification-items">
                      
                        <%
                        for(int i = 0; i < 30; i++) { %>
                        	<div class="media tm-notification-item">
                                <div class="tm-gray-circle">
                                	<img src="img/notification-01.jpg" alt="Avatar Image" class="rounded-circle">
                                </div>
	                            <div class="media-body">
	                                <p class="mb-2"><b><%=student_number[i] %></b></p>
	                                <p class="mb-2"><%=name[i] %></p>
	                                <p class="mb-2"><%=department[i] %></p>
	                            </div>
	                            <div class="percen">
	                            	<h1><span class="c_green">10</span>/12</h1>
	                            </div>
                            </div>
                        <% 
                        }
                        %>
                        

                        </div>
                    </div>
                </div>
                <div class="col-12 tm-block-col">
                    <div class="tm-bg-primary-dark tm-block tm-block-taller tm-block-scroll">
                    	<h2 class="tm-block-title">출석 현황</h2>
                
                        
                     
                        <table class="table">
                            <thead>
                                <tr>
                                	<th scope="col">
	                                	<select class="custom-select30">
	                                		<option value="0">ALL</option>
							                <option value="1">1 주차</option>
							                <option value="2">2 주차</option>
							                <option value="3">3 주차</option>
							                <option value="4">4 주차</option>
							           	</select>
							        </th>
                                    <th scope="col">학생 번호</th>
                                    <th scope="col">출석</th>
                                    <th scope="col">이름</th>
                                    <th scope="col">시간</th>
                                    <th scope="col">시간</th>
                                </tr>
                            </thead>
                            <tbody>
                            <%
                            	for(int i = 0; i < 30; i++) {
                            	%>
                            		<tr>
                                		<td>1주차</td>
                                    	<th scope="row"><b><%=student_number[i] %></b></th>
                                    	<td>
                                        	<div class="tm-status-circle1 moving">
                                        	</div>
                                    	</td>
                                    	<td><b><%=name[i] %></b></td>
                                    	<td>2019/07/01 12:16</td>
                                    	<td>2019/07/01 13:00</td>
                                	</tr>
                            	<%
                            	}
                            %>   
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        <footer class="tm-footer row tm-mt-small">
            <div class="col-12 font-weight-light">
                <p class="text-center text-white mb-0 px-4 small">
                    Copyright &copy; <b>2018</b> All rights reserved. 
                    
                    Design: <a rel="nofollow noopener" href="https://templatemo.com" class="tm-footer-link">Template Mo</a>
                </p>
            </div>
        </footer>
    </div>

    <script src="js/jquery-3.3.1.min.js"></script>
    <!-- https://jquery.com/download/ -->
    <script src="js/moment.min.js"></script>
    <!-- https://momentjs.com/ -->
    <script src="js/Chart.min.js"></script>
    <!-- http://www.chartjs.org/docs/latest/ -->
    <script src="js/bootstrap.min.js"></script>
    <!-- https://getbootstrap.com/ -->
    <script src="js/circle.js"></script>
    <script>
        Chart.defaults.global.defaultFontColor = 'white';
        let ctxPie,
            optionsPie,
            configPie,
			pieChart;
        // DOM is ready
        $(function() {
        	drawPieChart(); // Pie Chart
        })
    </script>
</body>

</html>