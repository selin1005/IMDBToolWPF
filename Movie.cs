namespace IMDbToolWPF
{
    using System;

    public class Movie
    {
        public string Title { get; set; }
        public double Rating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public string Genre { get; set; }

        public Movie()
        {
            this.ReleaseDate = DateTime.Now.AddDays(1);
            this.Year = DateTime.Now.Year + 1;
        }

        public override string ToString()
        {
            return Title + ", " + Rating;
        }
    }
}
