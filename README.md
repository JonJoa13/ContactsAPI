# ContactsAPI

In order to test this API, here is a few preparation steps:
1. The user need to have SQL Server installed. 
2. Open the solution on Visual Studio 2022.
3. The connectionString in the appsettings.json has to be updated.
   "ServerName" must be replaced by the local SQL Server name of the user.
4. In Visual Studio, launch command "Update-Database" from the Package Manager Console.
   It will create the database on SQL Server.
______
HOW IT WORKS:

api/User -> The first step is to create a user. The roles available are "Admin" or "Standard".
            The admin role give access to all CRUD functions.
            The standard role only give access to get functions.

api/User/Login -> Once the user is created, a token is needed to access all the other functions. The login will return the token.

Authorize button -> The token has to be pasted as Bearer to access all the Contacts and Skills functions.

