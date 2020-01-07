using System.ComponentModel;

namespace ShippingTools
{
    // 合成组
    public class Template : INotifyPropertyChanged
    {
        private string title;
        public string Title { get => title ?? ""; set { title = value; OnPropertyChanged("Title"); } }
        private string left;
        public string Left { get => left ?? ""; set { left = value; OnPropertyChanged("Left"); } }
        private string right;
        public string Right { get => right ?? ""; set { right = value; OnPropertyChanged("Right"); } }
        private string bottom;
        public string Bottom { get => bottom ?? ""; set { bottom = value; OnPropertyChanged("Bottom"); } }

        public object Info { get; set; }

        private string color;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Color { get => color ?? "#FF1F377F"; set => color = value; }

        private bool isSelected;
        public bool IsSelected { get { return isSelected; } set { isSelected = value;OnPropertyChanged(Background); } }
        public string Background { get => isSelected ? "" : "#FFFFFFFF"; }
    }
}
