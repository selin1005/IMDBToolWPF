namespace IMDbTool
{
    using System.Collections.Generic;

    class Actor
    {
        [System.ComponentModel.Bindable(true)]
        public string Name { get; set; }

        public Actor(string name, string photoURL)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
