using System.Windows;
using RestSharp;
using Be.Timvw.Framework.ComponentModel;
using System.Xml.Serialization;
using System.IO;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace IMDbToolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RestClient apiFilmClient;
        RestClient omdbClient;
        SortableBindingList<Movie> movies;
        SortableBindingList<string> actors;
        //PhotoBox photoBox;
        string actorName;
        BackgroundWorker actorBackgroundWorker;
        BackgroundWorker movieBackgroundWorker;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponent();
            movies = new SortableBindingList<Movie>();
            actors = new SortableBindingList<string>();
            apiFilmClient = new RestClient("http://www.myapifilms.com/");
            omdbClient = new RestClient("http://www.omdbapi.com");
            //photoBox = new PhotoBox();
            actorBackgroundWorker = new BackgroundWorker();
            actorBackgroundWorker.DoWork += bckGndWorkerFetchActors_DoWork;
            actorBackgroundWorker.RunWorkerCompleted += bckGndWorkerFetchActors_RunWorkerCompleted;
            movieBackgroundWorker = new BackgroundWorker();
            movieBackgroundWorker.DoWork += bckGndWorkerFetchMovies_DoWork;
            movieBackgroundWorker.RunWorkerCompleted += bckGndWorkerFetchMovies_RunWorkerCompleted;
        }

        private void GetActorList()
        {
            var request = new RestRequest("imdb/idIMDB", Method.GET);
            request.AddParameter("token", "573f54b5-bf92-440f-9652-764d6edc370a");
            request.AddParameter("format", "xml");
            request.AddParameter("name", actorName);
            request.AddParameter("filmography", 0);
            request.AddParameter("limit", 10);

            // execute the request
            IRestResponse response = apiFilmClient.Execute(request);
            var content = response.Content;
            var textReader = new StringReader(content);
            var xmlSerializer = new XmlSerializer(typeof(ActorResponse.results));
            var actorResponse = (ActorResponse.results)xmlSerializer.Deserialize(textReader);
            if (actorResponse.data != null)
            {
                for (int i = 0; i < actorResponse.data.Length; i++)
                {
                    actors.Add(actorResponse.data[i].name);
                }
            }
        }

        private void MaintainStart()
        {
            dataGridView1.Items.Clear();
            this.Cursor = Cursors.Wait;
            movies.Clear();
            //photoBox.Hide();
            btnFetch.IsEnabled = false;
            comboBox1.IsEnabled = false;
            textBoxActorName.IsEnabled = false;
        }

        private void MaintainEnd()
        {
            this.Cursor = Cursors.Arrow;
            btnFetch.IsEnabled = true;
            textBoxActorName.IsEnabled = true;
            comboBox1.IsEnabled = true;
        }

        private void GetActorInfo()
        {
            var request = new RestRequest("imdb/idIMDB", Method.GET);
            request.AddParameter("token", "573f54b5-bf92-440f-9652-764d6edc370a");
            request.AddParameter("format", "xml");
            request.AddParameter("name", actorName);
            request.AddParameter("filmography", 1);

            // execute the request
            IRestResponse response = apiFilmClient.Execute(request);
            var content = response.Content;
            var textReader = new StringReader(content);
            var xmlSerializer = new XmlSerializer(typeof(ActorResponse.results));
            var actorResponse = (ActorResponse.results)xmlSerializer.Deserialize(textReader);
            if (actorResponse.data != null && actorResponse.data[0].name != null)
            {
                var films = actorResponse.data[0].filmographies[0].filmographyData;
                for (int i = 0; i < films.Length; i++)
                {
                    GetMovieInfo(films[i].IMDBId);
                }

                //photoBox.SetUrl(actorResponse.data[0].urlPhoto);
            }
        }

        private void GetMovieInfo(string imdbId)
        {
            var request = new RestRequest();
            request.AddParameter("i", imdbId);
            request.AddParameter("plot", "short");
            request.AddParameter("r", "xml");
            request.AddParameter("apikey", "1276b923");

            // execute the request
            IRestResponse response = omdbClient.Execute(request);
            var content = response.Content;
            var textReader = new StringReader(content);
            var xmlSerializer = new XmlSerializer(typeof(MovieResponse.root));
            var movieResponse = (MovieResponse.root)xmlSerializer.Deserialize(textReader);

            if (movieResponse != null && movieResponse.movie != null)
            {
                var movie = new Movie()
                {
                    Title = movieResponse.movie[0].title,
                    Genre = movieResponse.movie[0].genre,
                    Type = movieResponse.movie[0].type,
                };

                double rating = double.MinValue;
                if (double.TryParse(movieResponse.movie[0].imdbRating, out rating))
                {
                    movie.Rating = rating;
                }

                int year = int.MinValue;
                if (int.TryParse(movieResponse.movie[0].year, out year))
                {
                    movie.Year = year;
                }

                DateTime releaseDate;
                if (DateTime.TryParse(movieResponse.movie[0].released, out releaseDate))
                {
                    movie.ReleaseDate = releaseDate;
                }
                else
                {
                    movie.ReleaseDate = new DateTime(movie.Year, 1, 1);
                }

                movies.Add(movie);
            }
        }

        //private void FormatCells()
        //{
        //    foreach (DataGridViewRow row in dataGridView1.Rows)
        //    {
        //        var movie = row.DataBoundItem as Movie;

        //        if (movie.Type.Equals("movie", StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (movie.Rating >= 56)
        //            {
        //                row.DefaultCellStyle.BackColor = Brushes.Green;
        //            }
        //            else if (movie.Rating >= 40)
        //            {
        //                row.DefaultCellStyle.BackColor = Brushes.White;
        //            }
        //            else if (movie.Rating > 0)
        //            {
        //                row.DefaultCellStyle.BackColor = Brushes.Red;
        //            }
        //            else
        //            {
        //                row.DefaultCellStyle.BackColor = Brushes.Gray;
        //            }

        //            if (movie.ReleaseDate > DateTime.Now | movie.Year > DateTime.Now.Year)
        //            {
        //                row.DefaultCellStyle.BackColor = Brushes.Gray;
        //            }
        //        }
        //    }
        //}

        //private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    FormatCells();
        //}

        private void bckGndWorkerFetchActors_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            comboBox1.ItemsSource = actors;
            MaintainEnd();
        }

        private void bckGndWorkerFetchActors_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GetActorList();
        }

        private void bckGndWorkerFetchMovies_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GetActorInfo();
        }

        private void bckGndWorkerFetchMovies_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            dataGridView1.ItemsSource = movies;
            //dataGridView1.Sort(dataGridView1.Columns[1], System.ComponentModel.ListSortDirection.Descending);
            //FormatCells();
            MaintainEnd();
            //photoBox.Text = actorName;
            //photoBox.Show();
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.textBoxActorName.Text.Trim()))
            {
                MaintainStart();
                actorName = this.textBoxActorName.Text.Trim();
                actors.Clear();
                comboBox1.Items.Clear();
                actorBackgroundWorker.RunWorkerAsync();
            }
        }

        private void comboBox1_SelectionChangeCommitted(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            actorName = comboBox1.SelectedItem as string;
            MaintainStart();
            movieBackgroundWorker.RunWorkerAsync();
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnFetch_Click(this, new RoutedEventArgs());
            }
        }
    }
}
