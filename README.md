# SpotifyAnalysis

SpotifyAnalysis is a work-in-progress project designed to analyze, chart and manage Spotify data. It includes tools for generating statistics, visualizations, and insights into playlists, tracks, and user profiles. 
#### Tech stack:
- .NET 8
- Blazor
- MudBlazor
- Spotify API
- Entity Framework Core
- Blazor-ApexCharts

## Features

- **Playlist and Track Management**:
  - Browse playlists and tracks with detailed metadata.
  - Identify duplicates and organize your Spotify library.

- **Data Visualization**:
  - Generate interactive charts to explore Spotify data.
  - Analyze trends across artists, genres, albums, and tracks.

- **Spotify Integration**:
  - Fetch personalized data from the Spotify API.
  - Supports user profiles, playlists, and track metadata.

- **Data Persistence**:
  - Manage and store data using Entity Framework Core with a local database.
  - Handles complex many-to-many relationships between playlists, tracks, albums, and artists.

## Project Structure

- **SpotifyAnalysis**: Main project containing application logic and UI components.
  - `Data/DTO`: Data transfer objects for Spotify entities like tracks, playlists, and artists.
  - `Data/Database`: Handles database operations with `SpotifyContext` and `DTOAggregate`.
  - `Data/SpotifyAPI`: Manages API integration for fetching Spotify data.
  - `Components`: Modular UI components built using MudBlazor.
  - `Pages`: Razor pages for user interaction.
  - `wwwroot`: Static assets including styles and images.

- **UnitTests**: Contains unit tests for data-fetching logic, Spotify API integration, and performance benchmarking.
 - `Program`: Runs a benchmark for evaluating the performance of the `GetData` method with large datasets.

## Getting Started

### Prerequisites

- .NET 8
- A local Microsoft SQL Server instance for database management.
- A Spotify Developer account with API credentials.

### Setup

1. **Database Configuration**:
   - The default connection string in `appsettings.json` is configured for Microsoft SQL Server:
     ```json
     "ConnectionStrings": {
       "SpotifyDB": "Server=localhost;Database=SpotifyDB;Integrated Security=true;TrustServerCertificate=True"
     }
     ```
   - To initialize the database schema, ensure SQL Server is running and execute:
     ```bash
     dotnet ef database update
     ```

2. **API Credentials**:
   - Set your Spotify API credentials via the CLI:
     ```bash
     dotnet user-secrets set "ClientId" "your-client-id"
     dotnet user-secrets set "ClientSecret" "your-client-secret"
     ```

3. **Run the Application**:
   Launch the application to explore its features.

### Testing and Benchmarking

- **Run Unit Tests**:
  Execute the unit tests:
```bash
dotnet test
```

 - **Performance Benchmarking**: The test project includes a benchmark for evaluating the performance of the `GetData` method with large datasets. This is an ETL method with potentially long run times. Run the benchmark by invoking the test project's `Main` entry point.

## Roadmap

- Interactive documentation on each page
- More advanced duplicate search
- Node graph for finding collaborating or similar artists
- Sorting and/or filtering tracks in playlists and reflecting the changes in Spotify
- Clustering tracks by parameters (danceability, energy, liveness) but these endpoints have just been deprecated
- Retaining selections across pages

## Contribution

Contributions are welcome! If you have suggestions or encounter issues, feel free to open an issue or submit a pull request.

## License

This project is licensed under a custom AGPL 3.0. Details can be found in the [LICENSE](LICENSE.md) file.
