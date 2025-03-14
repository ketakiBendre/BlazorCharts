# Blazor Charts Dashboard

**About the project:**  
This project focuses on creating and managing charts using MudBlazor. The charts are highly customizable, allowing users to drag, drop, and resize them effortlessly within the dashboard. Implementing this functionality without JavaScript was challenging, but with MudBlazor, it was achieved using simple and efficient code.

Such dashboards can serve various purposes, including analyzing battery data and other analytical insights. Using these charts in Business and Finances, Hospital Data etc. C# with MudBlazor made it easy to use. Users can directly use this Nuget Packge and customize as per the requirements.

**Demo:**

https://github.com/user-attachments/assets/719a21e7-e265-45bb-8e91-dbbef43eca8b
   
**Built with:**
 
   - [MudBlazor 8.3.0](https://www.mudblazor.com/)
   - [.net 8.0](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
   - [PostgreSQL](https://www.postgresql.org/)
   - [Mapbox](https://www.mapbox.com/)
   - Javascript

<!-- GETTING STARTED -->
## Getting Started
### Pre-requisites  
   Below packages should be already installed and ready to use:  
   - Microsoft.AspNetCore.Components.WebAssembly
   - Microsoft.Extensions.Http
   - MudBlazor 8.3.0
   - System.Text.Json
   - Dapper
   - Get the Mapbox access token and copy in `BlazorCharts.UI\wwwroot\appsettings.json` file
   - Get DB Connection string and copy in `BlazorCharts.API\appsettings.json
     
### **Installation:**
   - Clone Repository
     
     git clone [BlazorCharts](https://github.com/ketakiBendre/BlazorCharts)

   - Copy below DB schema in postgreSQL:
     
     [BlazorChartsDB.zip](https://github.com/user-attachments/files/19241428/BlazorChartsDB.zip)


### Usage:

This is a sample dashboard built using .NET and MudBlazor for analyzing battery and energy management in EVs.  
It serves as an analytical tool that simulates battery energy performance, eliminating the need for physical batteries.  
The dashboard includes various interactive charts for data visualization and maps that display battery locations,
enabling efficient monitoring and analysis.
