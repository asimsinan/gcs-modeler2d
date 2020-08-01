Demo application is available at http://gcsmodeler2d.site
# What is GCS-Modeler2D?
GCS-Modeler2D stands for Geotechnical Cross-Section Modeler 2D. It is an online data visualization for modeling geotechnical cross-sections. In real life, the visualization process is done by hand either on a pice of paper or on a PC by using a drawing tool. Visualizations vary depending on the researcher’s academic knowledge, intuition and drawing ability. For this reason, we've developed an application that standardizes the process and has the ability to visualize cross-sections fast and in a practical way. GCS-Modeler2D can automatically generate geotechnical cross-sections with the mean visualization accuracy of the system was calculated as 88.39%. It takes the electrical resistivity, seismic velocity and mechanical drilling values as input and outputs an underground cross-section. Cross-sections can also be edited by hand, if there are errors on the visualization.

# Team
* **Asst. Prof. Dr. Asım Sinan Yüksel:** Süleyman Demirel University Department of Computer Engineering, Algorithm Design, Architectural Design, Testing
* **Kaan Er:** Süleyman Demirel University Department of Computer Engineering, Coding, Coding Front-End and Back-End
* **Assoc. Prof. Dr. Osman Uyanık:** Süleyman Demirel University Department of Geophysics Engineering, Theorical Foundations of Geotechnical Cross-Sections, Testing

# Technology behind GCS-Modeler2D
We built GCS-Modeler2D on various cutting-edge open source technologies as listed below. 
- We used Angular as the front-end web framework.
- Bootstrap and HTML for the design of typography, forms, buttons and tables.
Storage and management of rules and visualization data was done with Microsoft SQL Server with entity framework that enables us to work with data using objects of domain specific classes. However, currently entity framework is disabled (session is enabled) and related codes are commented for easy of use. It can be re-enabled for further development.
- The back-end of GCS-Modeler2D was written in C# language and .NET was used as server-side web-application framework.
- We also used technologies such as Javascript and JSON to make pages more interactive with the user, providing asynchronous information requests. 
- Highcharts Javascript library was used for visualization of geotechnical cross-sections. This charting library is written in pure JavaScript and offers an easy way of adding interactive cross-sections to our web application.
- Online access via web browsers is done with Internet Information Services (ISS) web server, which is a flexible, secure and manageable web server for hosting anything on the web.
- We used Microsoft Visual Studio 2017 as the software development IDE.

# How to run the software
  0. Prerequisites: Visual Studio 2017 IDE, a Windows based hosting that supports .NET and IIS enabled.
  1. You can use the publish tool from Visual Studio to publish this project to a website.
  2. Open the FuzzyMsc.sln file with Visual Studio.
  3. In Solution Explorer, right-click on the master project and choose Publish.
  4. In the Publish dialog box, choose Web Server (IIS).
  5. Choose Web Deploy as the deployment method.
  6. Configure the settings (Website name, username, password etc) for the publish method and select Finish.
  7. Go to your website by typing the url and start using the application.
  
 # Dataset
 You can use [this dataset](https://data.mendeley.com/datasets/pnd5789gyz/1) to try the application.
 
 # Youtube Video
 Below is the video of the working product.
 
 [![](http://img.youtube.com/vi/UnUwVP-Ulfs/0.jpg)](http://www.youtube.com/watch?v=UnUwVP-Ulfs "")
 
# Comparison of Hand-Drawn Cross-Section and Cross-Section Visualized With GCS-Modeler2D
 
## Dataset-1
 
Hand-Drawn Cross-Section

Cross-Section Visualized With GCS-Modeler2D

## Dataset-2
 
Hand-Drawn Cross-Section

Cross-Section Visualized With GCS-Modeler2D

## Dataset-3
 
Hand-Drawn Cross-Section

Cross-Section Visualized With GCS-Modeler2D




