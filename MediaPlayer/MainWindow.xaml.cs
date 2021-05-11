using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls.Primitives;

namespace MediaPlayer
{

    public partial class MainWindow : Window
    {
        public int pp = 0;
        public string listfilename;
        public bool fullscreen = false;

        public List<string> Medias = new List<string>();
        private bool userIsDraggingTime = false;

        private bool isPlaying = false;

        OpenFileDialog OpenFileDia = new OpenFileDialog();

        public object SelectedItem { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                if (MediaPlayer.Position == TimeSpan.FromSeconds(TimeLine.Maximum)) AutoPlay();
                if (MediaPlayer.NaturalDuration.HasTimeSpan && !userIsDraggingTime)
                {
                    TimeLine.Minimum = 0;
                    TimeLine.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    TimeLine.Value = MediaPlayer.Position.TotalSeconds;
                    Title.Content = Path.GetFileName(MediaPlayer.Source.ToString());
                }
                else Title.Content = "No file selected...";
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Megtalálja a listában majd pedig az előtte álló indexszel elindítja ha nincs akkor pedig a legnagyobbat tehát a sor végére megy
                int index = 0;
                //Amennyiben nincs elindítva semmi akkor le sem fut (Mediaplayer?)
                if (!(MediaPlayer.Source == null))
                {
                    //Végig olvassa a listát, hogy hol lehet a fájl ami éppen fut
                    foreach (var item in Medias)
                    {
                        //Lista fájl neve
                        string[] asd1 = item.Split('\\');
                        //Éppen futó fájl neve
                        string[] asd2 = MediaPlayer.Source.ToString().Split('/');
                        //Megtalálja az eggyezést
                        if (asd1[asd1.Length - 1] == asd2[asd2.Length - 1])
                        {
                            //ha megvan akkor elmenti az indexet és kilép a ciklusból
                            index = Medias.IndexOf(item);
                            break;
                        }
                    }

                    if (index - 1 < 0)
                    {
                        MediaPlayer.Source = new Uri(Medias[Medias.Count - 1]);
                    }
                    else
                    {
                        MediaPlayer.Source = new Uri(Medias[index - 1]);
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MediaPlayer.Source == null && !isPlaying)
                {
                    MediaPlayer.Source = new Uri(Medias.First());
                    MediaPlayer.Play();
                }
                if (isPlaying)
                {
                    MediaPlayer.Pause();
                    isPlaying = false;
                }
                else
                {
                    MediaPlayer.Play();
                    isPlaying = true;
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Nincs fájl beimportálva");
                Console.WriteLine(error.Message);
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            AutoPlay(); 
        }
        private void MediaList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = 0;
            foreach (var item in Medias)
            {
                if (item.Contains(MediaList.SelectedItem.ToString()))
                    index = Medias.IndexOf(item);
            }
            listfilename = Medias[index];
            MediaPlayer.Source = new Uri(listfilename);
            MediaPlayer.Play();
            isPlaying = true;
        }
        //TIMELINE
        private void TimeLine_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingTime = true;
        }
        private void TimeLine_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingTime = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(TimeLine.Value);
        }
        private void TimeLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TitleTime.Text = TimeSpan.FromSeconds(TimeLine.Value).ToString(@"hh\:mm\:ss") + " / " + MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
        }
        private void VoiceLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Volume = (double)VoiceLine.Value;
            
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDia.Filter = "MP3 files (*.mp3)|*.mp3|Wav files (*.wav)|*.wav|MP4 files (*.mp4)|*.mp4|Mkv files (*.mkv)|*.mkv|All files (*.*)|*.*";
            OpenFileDia.Multiselect = true;
            OpenFileDia.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (OpenFileDia.ShowDialog() == true)
            {
                foreach (string filename in OpenFileDia.FileNames)
                {
                    if (!(Medias.Contains(filename)))
                    {
                        Medias.Add(filename);
                    }
                }
            }
            InsertListItemsToListBox();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = 0;
                if (MediaList.SelectedItem != null)
                {
                    foreach (var item in Medias)
                    {
                        if (item.Contains(MediaList.SelectedItem.ToString()))
                            index = Medias.IndexOf(item);
                    }
                }
                listfilename = Medias[index];
                Console.WriteLine(listfilename);
                Medias.Remove(listfilename);
                InsertListItemsToListBox();
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Nincs mit kitörölni a listából.");
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

        }
        private void OpenListButton_Click(object sender, RoutedEventArgs e)
        {
            //Kivételt kezelünk hiba esetére
            try
            {
                using (StreamReader sr = new StreamReader(@"MusicList.txt"))
                {
                    string line;
                    //Amíg nem üres a sor addig olvassuk a fájlt.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (Medias.Contains(line))
                        {
                            Console.WriteLine($"Már szerepel a Listában: {line}");
                        }
                        else
                        {
                            //Minden sort hozzá adunk a listához
                            Medias.Add(line);
                        }
                    }
                }
                //Meghívjuk azt a metódust amivel beolvassa a lista elemeit a Listboxba
                InsertListItemsToListBox();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
        private void SaveListButton_Click(object sender, RoutedEventArgs e)
        {
            //Kivételt kezelünk hiba esetére
            try
            {
                using (StreamWriter sw = new StreamWriter(@"MusicList.txt"))
                {
                    //Végig olvassuk a Medias listát és kiírjuk egy fájlba
                    foreach (var item in Medias)
                    {
                        sw.WriteLine(item);
                    }
                }
                MessageBox.Show("Lista elmentve.");
            }
            catch (Exception error)
            {
                Console.WriteLine("The file could not be read:");
                MessageBox.Show(error.Message);
            }
        }
        private void InsertListItemsToListBox()
        {
            MediaList.Items.Clear();
            //Végig olvassa a listát
            foreach (var item in Medias)
            {
                //Beilleszti a List tartamát a ListBoxba
                MediaList.Items.Add(Path.GetFileName(item));
            }
        }

        private void MediaPlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2 && fullscreen == false)
            //{
            //    this.Content = MediaPlayer;
            //    this.WindowStyle = WindowStyle.None;
            //    this.WindowState = WindowState.Maximized;
            //    MessageBox.Show(e.ClickCount.ToString());
            //}
            //else if (e.ClickCount == 2 && fullscreen == true)
            //{

            //    this.WindowStyle = WindowStyle.SingleBorderWindow;
            //    this.WindowState = WindowState.Normal;
            //}
            //fullscreen = !fullscreen;
        }

        private void AutoPlay()
        {
            try
            {
                //Megtalálja a listában majd pedig az előtte álló indexszel elindítja ha nincs akkor pedig a legnagyobbat tehát a sor végére megy
                int index = 0;
                //Amennyiben nincs elindítva semmi akkor le sem fut (Mediaplayer?)
                if (!(MediaPlayer.Source == null))
                {
                    //Végig olvassa a listát, hogy hol lehet a fájl ami éppen fut
                    foreach (var item in Medias)
                    {
                        //Lista fájl neve
                        string[] asd1 = item.Split('\\');
                        //Éppen futó fájl neve
                        string[] asd2 = MediaPlayer.Source.ToString().Split('/');
                        //Megtalálja az eggyezést
                        if (asd1[asd1.Length - 1] == asd2[asd2.Length - 1])
                        {
                            //ha megvan akkor elmenti az indexet és kilép a ciklusból
                            index = Medias.IndexOf(item);
                            break;
                        }
                    }
                    Console.WriteLine(index + 1 > Medias.Count - 1);
                    if (index + 1 > Medias.Count - 1)
                    {
                        Console.WriteLine(Medias[0]);
                        MediaPlayer.Source = new Uri(Medias[0]);
                    }
                    else
                    {
                        Console.WriteLine(Medias[index + 1]);
                        MediaPlayer.Source = new Uri(Medias[index + 1]);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}