package student;


import java.sql.*;


public class jdbc {

	 public static void main(String[] args) throws ClassNotFoundException {
	       String url = "jdbc:sqlserver://hanium.database.windows.net:1433;databaseName=Face;";
	        String user = "FaceAdmin@hanium";
	        String password = "FaceAPI@2019";
	        
	        try {
	           Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
	           
	           Connection conn = DriverManager.getConnection(url, user, password);
	           Statement stmt = conn.createStatement();
	           
	           System.out.println("MS-SQL 서버 접속에 성공하였습니다.");
	           
	           ResultSet rs = stmt.executeQuery("SELECT * FROM subject");
	           while (rs.next()) {
	              String field1 = rs.getString("subject_name");
	              String field2 = rs.getString("professor");
	              String field3 = rs.getString("subject_id");
	              System.out.print(field1 + "\t");
	              System.out.print(field2 + "\t");
	              System.out.println(field3);
	           }
	           
	           rs.close();
	           stmt.close();
	           conn.close();
	        } catch (SQLException sqle) {
	           System.out.println("SQLException : " + sqle);
	        }
	    }
	}
