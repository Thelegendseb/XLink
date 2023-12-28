using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;

using SpotifyAPI.Web;
using Microsoft.Extensions.Configuration;

using XLink.Utility;
using XLink.Actions;


namespace XLink.Context.Contexts
{
    public class con_Spotify : Context
    {

        SpotifyClient Spotify;
        string AccessToken;
        string RefreshToken;
        Timer RefreshTimer;
        Dictionary<string, string> user_playlists;

        // summary: The constructor for the context
        // param: string name - the name of the context
        public con_Spotify() : base("Spotify", ContextType.Application)
        { }

        // summary: Initialize the context
        protected override void Init()
        {

            string clientId = this.GetConfigValue("ClientId");
            string clientSecret = this.GetConfigValue("ClientSecret");
            string redirectUri = this.GetConfigValue("RedirectUri");
            string scope = this.GetConfigValue("Scopes");
            string userId = this.GetConfigValue("UserId");

            var tokens = Task.Run(async () => await GetTokens(clientId, clientSecret, redirectUri, scope)).Result;

            this.AccessToken = tokens.Item1;
            this.RefreshToken = tokens.Item2;
            int duration = int.Parse(tokens.Item3);
            duration = (duration * 5) / 6; // Refresh 10 minutes before token expires

            this.RefreshTimer = new Timer(new TimerCallback(async (e) => await RefreshAccessToken(clientId, clientSecret)));
            this.RefreshTimer.Change(duration * 1000, duration * 1000);

            this.Spotify = new SpotifyClient(this.AccessToken);

            this.user_playlists = Task.Run(async () => await LoadPlaylists(this.Spotify, userId)).Result;

        }

        // summary: Load the config values for the context
        // param: IConfigurationSection section - the section of the config file for the context
        // returns: Dictionary<string, string> - the config values for the context
        protected override Dictionary<string, string> LoadConfigValues(IConfigurationSection section)
        {

            Dictionary<string, string> config = new Dictionary<string, string>();

            var clientId = section["ClientId"];
            var clientSecret = section["ClientSecret"];
            var redirectUri = section["RedirectUri"];
            var DeviceId = section["DeviceId"];
            var scopes = section["Scopes"];
            var userId = section["UserId"];

            config.Add("ClientId", clientId);
            config.Add("ClientSecret", clientSecret);
            config.Add("RedirectUri", redirectUri);
            config.Add("DeviceId", DeviceId);
            config.Add("Scopes", scopes);
            config.Add("UserId", userId);

            return config;

        }

        // summary: Load the actions for the context
        // returns: Dictionary<string, ActionSchema> - the actions for the context
        protected override Dictionary<XAction.RequestSchema, XAction.ResponseSchema> LoadActions()
        {

            var actions = new Dictionary<XAction.RequestSchema, XAction.ResponseSchema>();
            actions[new XAction.RequestSchema() 
            { Name = "play", ReturnsResult = false, RequiresArgs = false }] = PlayAction;
            actions[new XAction.RequestSchema() 
            { Name = "pause", ReturnsResult = false, RequiresArgs = false }] = PauseAction;
            actions[new XAction.RequestSchema() 
            { Name = "next", ReturnsResult = false, RequiresArgs = false }] = NextAction;
            actions[new XAction.RequestSchema() 
            { Name = "previous", ReturnsResult = false, RequiresArgs = false}] = PreviousAction;
            actions[new XAction.RequestSchema() 
            { Name = "setvolume", ReturnsResult = false, RequiresArgs = true }] = SetVolumeAction;
            actions[new XAction.RequestSchema() 
            { Name = "increasevolume", ReturnsResult = false, RequiresArgs = false}] = IncreaseVolumeAction;
            actions[new XAction.RequestSchema() 
            { Name = "decreasevolume", ReturnsResult = false, RequiresArgs = false }] = DecreaseVolumeAction;
            actions[new XAction.RequestSchema() 
            { Name = "playplaylist", ReturnsResult = false, RequiresArgs = true }] = PlayPlaylistAction;
            actions[new XAction.RequestSchema() 
            { Name = "playsearch", ReturnsResult = false, RequiresArgs = true }] = PlaySearchAction;
            actions[new XAction.RequestSchema() 
            { Name = "likecurrentsong", ReturnsResult = false, RequiresArgs = false }] = LikeCurrentSongAction;
            actions[new XAction.RequestSchema() 
            { Name = "seekto", ReturnsResult = false, RequiresArgs = true }] = SeekToAction;
            actions[new XAction.RequestSchema() 
            { Name = "getcurrentsong", ReturnsResult = true, RequiresArgs = false }] = GetCurrentSongAction;
            actions[new XAction.RequestSchema() 
            { Name = "getqueue", ReturnsResult = true, RequiresArgs = false }] = GetQueueAction;
            actions[new XAction.RequestSchema() 
            { Name = "getplaylists", ReturnsResult = true, RequiresArgs = false }] = GetPlaylistsAction;
            return actions;   
            
        }

        // ==================== Actions ====================
        private XActionResponse PlayAction(string args)
        {
            try
            {
                this.Spotify.Player.ResumePlayback().GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "play", args, true, null, "Playing");
            }
            catch (Exception ex)
            {
                Logger.Log("Error playing track: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "play", args, false, "Error playing track: " + ex.Message, null);
            }      
        }

        private XActionResponse PauseAction(string args)
        {
            try
            {
                this.Spotify.Player.PausePlayback().GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "pause", args, true, null, "Paused");
            }
            catch (Exception ex)
            {
                Logger.Log("Error pausing track: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "pause", args, false, "Error pausing track: " + ex.Message, null);
            }
        }

        private XActionResponse NextAction(string args)
        {
            try
            {
                this.Spotify.Player.SkipNext().GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "next", args, true, null, "Skipped to next track");
            }
            catch (Exception ex)
            {
                Logger.Log("Error Skipping track: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "next", args, false, "Error Skipping track: " + ex.Message, null);
            }
        }

        private XActionResponse PreviousAction(string args)
        {
            try
            {
                this.Spotify.Player.SkipPrevious().GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "previous", args, true, null, "Skipped to previous track");
            }
            catch (Exception ex)
            {
                Logger.Log("Error Skipping back track: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "previous", args, false, "Error Skipping back track: " + ex.Message, null);
            }
        }

        private XActionResponse SetVolumeAction(string args)
        {
            try
            {
                int volnew = int.Parse(args);
                if (volnew > 100)
                {
                    volnew = 100;
                }
                if (volnew < 0)
                {
                    volnew = 0;
                }
                this.Spotify.Player.SetVolume(new PlayerVolumeRequest(volnew)).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "setvolume", args, true, null, "Volume set to " + volnew.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log("Error setting volume: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "setvolume", args, false, "Error setting volume: " + ex.Message, null);
            }
        }

        private XActionResponse IncreaseVolumeAction(string args)
        {
            try
            {
                if (args == "")
                {
                    args = "10";
                }
                CurrentlyPlayingContext context = this.Spotify.Player.GetCurrentPlayback().GetAwaiter().GetResult();
                int volnew = (int)context.Device.VolumePercent + int.Parse(args);
                if (volnew > 100)
                {
                    volnew = 100;
                }
                if (volnew < 0)
                {
                    volnew = 0;
                }
                this.Spotify.Player.SetVolume(new PlayerVolumeRequest(volnew)).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "increasevolume", args, true, null, "Volume increased to " + volnew.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log("Error increasing volume: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "increasevolume", args, false, "Error increasing volume: " + ex.Message, null);
            }
        }

        private XActionResponse DecreaseVolumeAction(string args)
        {
            try
            {
                if (args == "")
                {
                    args = "10";
                }
                CurrentlyPlayingContext context = this.Spotify.Player.GetCurrentPlayback().GetAwaiter().GetResult();
                int volnew = (int)context.Device.VolumePercent - int.Parse(args);
                if (volnew > 100)
                {
                    volnew = 100;
                }
                if (volnew < 0)
                {
                    volnew = 0;
                }
                this.Spotify.Player.SetVolume(new PlayerVolumeRequest(volnew)).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "decreasevolume", args, true, null, "Volume decreased to " + volnew.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log("Error decreasing volume: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "decreasevolume", args, false, "Error decreasing volume: " + ex.Message, null);
            }
        }

        private XActionResponse PlayPlaylistAction(string args)
        {
          try
            {
                string playlistId = this.user_playlists[args];

                if (playlistId == "likedsongs")
                {

                    // Get all liked songs
                    int offset = 0;
                    int limit = 50;
                    List<FullTrack> alltracks = new List<FullTrack>();

                    while (true)
                    {
                        Paging<SavedTrack> tracks = this.Spotify.Library.GetTracks(new LibraryTracksRequest()
                        {
                            Limit = limit,
                            Offset = offset
                        }, default).GetAwaiter().GetResult();
                        foreach (SavedTrack track in tracks.Items)
                        {
                            alltracks.Add(track.Track);
                        }
                        if (tracks.Items.Count < limit)
                        {
                            break;
                        }
                        offset += limit;
                    }

                    if (alltracks.Any())
                    {
                        // Get URIs of liked songs
                        var trackUris = alltracks.Select(track => track.Uri).ToList();

                        // Start playing the liked songs
                        this.Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
                        {
                            Uris = trackUris,
                        }).GetAwaiter().GetResult();
                        return new XActionResponse(this.GetName(), "playplaylist", args, true, null, "Playing liked songs");
                    }
                    else
                    {
                        Logger.Log("No liked songs found", LogLevel.Warning);
                        return new XActionResponse(this.GetName(), "playplaylist", args, false, "No liked songs found", null);
                    }
                }
                else
                {
                    string playlistUri = $"spotify:playlist:{playlistId}";
                    this.Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
                    {
                        ContextUri = playlistUri,
                    }).GetAwaiter().GetResult();
                    return new XActionResponse(this.GetName(), "playplaylist", args, true, null, "Playing playlist " + args);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error playing playlist: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "playplaylist", args, false, "Error playing playlist: " + ex.Message, null);
            }

        }

        private XActionResponse PlaySearchAction(string args)
        {
            try
            {
                SearchRequest request = new SearchRequest(SearchRequest.Types.Track, args);
                request.Limit = 1;
                SearchResponse response = this.Spotify.Search.Item(request).GetAwaiter().GetResult();
                string uri = response.Tracks.Items[0].Uri;
                this.Spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest()
                {
                    Uris = new List<string>() { uri },
                }).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "playsearch", args, true, null, "Playing " + args);
            }
            catch (Exception ex)
            {
                Logger.Log("Error playing song: " + ex.Message, LogLevel.Error);
               return new XActionResponse(this.GetName(), "playsearch", args, false, "Error playing song: " + ex.Message, null);
            }
        }

        private XActionResponse LikeCurrentSongAction(string args)
        {
            try
            {
                PlayerCurrentlyPlayingRequest request = new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track);
                CurrentlyPlaying currentlyPlaying = this.Spotify.Player.GetCurrentlyPlaying(request).GetAwaiter().GetResult();
                FullTrack playableItem = (FullTrack)currentlyPlaying.Item;
                this.Spotify.Library.SaveTracks(new LibrarySaveTracksRequest(new List<string>() { playableItem.Id })).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "likecurrentsong", args, true, null, "Liked song");
            }
            catch (Exception ex)
            {
                Logger.Log("Error liking song: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "likecurrentsong", args, false, "Error liking song: " + ex.Message, null);
            }
        }

        private XActionResponse SeekToAction(string args)
        {
            try
            {
                PlayerCurrentlyPlayingRequest request = new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track);
                CurrentlyPlaying currentlyPlaying = this.Spotify.Player.GetCurrentlyPlaying(request).GetAwaiter().GetResult();
                FullTrack playableItem = (FullTrack)currentlyPlaying.Item;
                int duration = playableItem.DurationMs;
                int percentage = int.Parse(args);
                int milliseconds = (duration * percentage) / 100;
                this.Spotify.Player.SeekTo(new PlayerSeekToRequest(milliseconds)).GetAwaiter().GetResult();
                return new XActionResponse(this.GetName(), "setplaybackposition", args, true, null, "Set playback position to " + args + "%");
            }
            catch (Exception ex)
            {
                Logger.Log("Error setting playback position: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "setplaybackposition", args, false, "Error setting playback position: " + ex.Message, null);
            }
        }

        private XActionResponse GetCurrentSongAction(string args)
        {
            try
            {
                PlayerCurrentlyPlayingRequest request = new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track);
                CurrentlyPlaying currentlyPlaying = this.Spotify.Player.GetCurrentlyPlaying(request).GetAwaiter().GetResult();
                FullTrack playableItem = (FullTrack)currentlyPlaying.Item;
                return new XActionResponse(this.GetName(), "getcurrentsong", args, true, null, SongToString(playableItem));
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting current song: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "getcurrentsong", args, false, "Error getting current song: " + ex.Message, null);
            }
        }

        private XActionResponse GetQueueAction(string args)
        {
            try
            {
                QueueResponse response = this.Spotify.Player.GetQueue().GetAwaiter().GetResult();
                string queue = "";
                for (int i = 0; i < response.Queue.Count; i++)
                {
                    string song = SongToString((FullTrack)response.Queue[i]);
                    queue += song + "\n";
                }
                return new XActionResponse(this.GetName(), "getqueue", args, true, null, queue);
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting queue: " + ex.Message, LogLevel.Error);
                return new XActionResponse(this.GetName(), "getqueue", args, false, "Error getting queue: " + ex.Message, null);
            }
        }

        private XActionResponse GetPlaylistsAction(string args)
        {
            try
            {
                string playlists = "";
                foreach (KeyValuePair<string, string> playlist in this.user_playlists)
                {
                    playlists += playlist.Key + "\n";
                }
                return new XActionResponse(this.GetName(), "getplaylists", args, true, null, playlists);
            }
            catch (Exception ex)
            {
                Logger.Log("Error getting playlists: " + ex.Message, LogLevel.Error);
                return new XActionResponse( this.GetName(), "getplaylists", args, false, "Error getting playlists: " + ex.Message, null);
            }
        }


        // ==================== Context Helpers ====================

            // summary: Load all playlists into a dictionary
            // param: SpotifyClient spotify - the spotify client
            // param: string userId - the user id
            // returns: Dictionary<string, string> - the playlists
        static async Task<Dictionary<string, string>> LoadPlaylists(SpotifyClient spotify, string userId)
        {
            int offset = 0;
            int limit = 50;
            List<FullPlaylist> allplaylists = new List<FullPlaylist>();

            while (true)
            {
                PlaylistGetUsersRequest request = new PlaylistGetUsersRequest();
                request.Limit = limit;
                request.Offset = offset;
                Paging<FullPlaylist> playlists = await spotify.Playlists.GetUsers(userId, request, default);
                allplaylists.AddRange(playlists.Items);
                if (playlists.Items.Count < limit)
                {
                    break;
                }
                offset += limit;
            }
            Dictionary<string, string> myplaylists = new Dictionary<string, string>();
            myplaylists.Add("liked songs", "likedsongs");
            for (int i = 0; i < allplaylists.Count; i++)
            {
                myplaylists.Add(allplaylists[i].Name.ToLower(), allplaylists[i].Id);
            }
            return myplaylists;

        }

        // summary: Get a string representation of a song
        // param: FullTrack track - the track to get the string representation of
        // returns: string - the string representation of the song
        static string SongToString(FullTrack track)
        {
            string song = track.Name + " | ";
            string artists = null;
            foreach (SimpleArtist artist in track.Artists)
            {
                artists += artist.Name + ", ";
            }
            song += artists.Substring(0, artists.Length - 2);
            return song;
        }

        // summary: Refresh the access token
        // param: string clientId - the client ID
        // param: string clientSecret - the client secret
        // returns: Task<string> - the access token
        async Task<bool> RefreshAccessToken(string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(this.RefreshToken))
            {
                // Handle case where refresh token is not available#
                Logger.Log("Refresh token is not available.", LogLevel.Error);
                return false;
            }

            // Specify the Spotify token endpoint URI for refreshing tokens
            string tokenEndpoint = "https://accounts.spotify.com/api/token";

            // Set up the request body with grant_type=refresh_token
            var requestBody = new StringContent(
                $"grant_type=refresh_token" +
                $"&client_id={clientId}" +
                $"&client_secret={clientSecret}" +
                $"&refresh_token={this.RefreshToken}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage tokenResponse = await client.PostAsync(tokenEndpoint, requestBody);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                    string[] strings = tokenResponseContent.Split('"');
                    string accessToken = strings[3];
                    string refreshToken = strings[13];
                    string duration = strings[10];
                    duration = duration.Substring(1, duration.Length - 2);
                    int duration_int = int.Parse(duration) * 5 / 6;
                    this.RefreshTimer.Change(duration_int * 1000, duration_int * 1000);
                    Logger.Log("New Spotify Access Token: " + accessToken.Substring(0, 15) + "...", LogLevel.Info);
                    this.Spotify = new SpotifyClient(accessToken);             
                    
                    this.AccessToken = accessToken;
                    this.RefreshToken = refreshToken;

                    Logger.Log($"Re-Initialized Spotify Client.", LogLevel.Info);
                    return true;
                }
                else
                {
                    Logger.Log($"Error: {tokenResponse.StatusCode} - {tokenResponse.ReasonPhrase}", LogLevel.Error);
                    return false;
                }
            }
        }


        // summary: Get an access token using PKCE (Proof Key for Code Exchange)
        // param: string clientId - the client ID
        // param: string clientSecret - the client secret
        // param: string redirectUri - the redirect URI
        // returns: Task<string> - the access token
        static async Task<Tuple<string, string, string>> GetTokens(string clientId, string clientSecret, string redirectUri, string scope)
        {
            // Generate a random code verifier and derive the code challenge
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);

            // Step 1: Redirect the user to Spotify's Authorization URL with PKCE parameters
            var authorizationUrl = $"https://accounts.spotify.com/authorize" +
                $"?client_id={clientId}" +
                $"&response_type=code" +
                $"&redirect_uri={redirectUri}" +
                $"&scope={scope}" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method=S256";

            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri + "/");
            listener.Start();

            Process proc = OpenBrowser(authorizationUrl);

            // Step 2: Wait for the redirect and extract the authorization code
            HttpListenerContext context = await listener.GetContextAsync();
            HttpListenerRequest request = context.Request;

            // Read the authorization code from the args parameters
            string authorizationCode = request.QueryString["code"];

            // Stop the HTTP listener
            listener.Stop();
            proc.Kill();
            // Step 2: Exchange the authorization code for an access token
            return await ExchangeCodeForToken(clientId, clientSecret, redirectUri, codeVerifier, authorizationCode);
        }

        // summary: Exchange the authorization code for an access token
        // param: string clientId - the client ID
        // param: string clientSecret - the client secret
        // param: string redirectUri - the redirect URI
        // param: string codeVerifier - the code verifier
        // param: string authorizationCode - the authorization code
        // returns: Task<string> - the access token
        static async Task<Tuple<string, string, string>> ExchangeCodeForToken(string clientId, string clientSecret, string redirectUri, string codeVerifier, string authorizationCode)
        {
            // Specify the Spotify token endpoint URI
            string tokenEndpoint = "https://accounts.spotify.com/api/token";

            // Set up the request body with grant_type=authorization_code
            var requestBody = new StringContent(
                $"grant_type=authorization_code" +
                $"&client_id={clientId}" +
                $"&client_secret={clientSecret}" +
                $"&redirect_uri={redirectUri}" +
                $"&code={authorizationCode}" +
                $"&code_verifier={codeVerifier}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            // Send the POST request to the token endpoint URI
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage tokenResponse = await client.PostAsync(tokenEndpoint, requestBody);

                // Check if the token request was successful (HTTP status code 200-299)
                if (tokenResponse.IsSuccessStatusCode)
                {
                    // Read and parse the response content to get the access token
                    string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                    string[] strings = tokenResponseContent.Split('"');
                    string accessToken = strings[3];
                    string refreshToken = strings[13];
                    string duration = strings[10];
                    duration = duration.Substring(1, duration.Length - 2);
                    Logger.Log($"Access token expires in {duration} seconds.", LogLevel.Info);
                    Tuple<string, string, string> tokens = new Tuple<string, string, string>(accessToken, refreshToken, duration);
                    return tokens;
                }
                else
                {
                    // Handle error cases
                    Console.WriteLine($"Error: {tokenResponse.StatusCode} - {tokenResponse.ReasonPhrase}");
                    return null;
                }
            }
        }

        // summary: Helper method to generate a random code verifier
        // returns: string - the code verifier
        static string GenerateCodeVerifier()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[32];
                rng.GetBytes(bytes);
                return Base64UrlEncode(bytes);
            }
        }

        // summary: Helper method to generate a code challenge from a code verifier
        // param: string codeVerifier - the code verifier
        // returns: string - the code challenge
        static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        // summary: Helper method to base64 URL encode a byte array
        // param: byte[] bytes - the byte array to encode
        // returns: string - the base64 URL encoded string
        static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        // summary: Open a browser window
        // param: string url - the URL to open
        // returns: Process - the process of the browser window
        static Process OpenBrowser(string url)
        {
            Process proc;
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                proc = Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception ex)
            {
                proc = null;
                Console.WriteLine($"Error opening browser: {ex.Message}");
            }
            return proc;
        }

    }

}

