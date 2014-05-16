# Coin Exchange Backend - README
#Setting up RavenDB
1. Download the RavenDB from here: http://hibernatingrhinos.com/downloads/RavenDB%20Installer/2879
2. Run the Setup installation.
3. While installing the RavenDB, please select "Development" in Tragetting environment.
4. Install the RavenDB as a service and chose enter port 8081.
5. After the installtion completes, open "http://localhost:8081/raven/studio.html" link in the browser.
6. In the main screen,click on "New Database", type "InputEventStore" in Name Field and click next.
7. In the main screen,click on "New Database", type "OutputEventStore" in Name Field and click next.
This all was needed to configure RavenDB. If the link "http://localhost:8081/raven/studio.html" make sure that RavenDB service is running.

#MySql
1. Download latest version of MySql community server
2. Run .exe or .msi file and follow the instructions.
3. Select Developer default.
4. There should be no password for root user.
5. Execute the "create" and "populate" scripts in the src\data\MySql folder.

#Database Setup
1. Create a new database named coinexchange.
2. MySql Settings:
3. username = root,
4. host = localhost,
5. port = 3306.

#MySQL Connector for .NET:
Download and install the MySQL connector for .NET from here: http://dev.mysql.com/downloads/connector/net/