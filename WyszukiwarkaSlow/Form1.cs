using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using static System.Windows.Forms.LinkLabel;

namespace WyszukiwarkaSlow
{
  

    public partial class Form1 : Form
    {

        public class AsmProxy
        {
            [DllImport("AsmSzukaj.dll")]
            private static extern int AsmStart(int mdlug_s, string mszuk, string mlancuch, int m_dlug_l);
            public int executeAsmStart(int mdlug_s, string mszuk, string mlancuch, int m_dlug_l)
            {
                return AsmStart(mdlug_s, mszuk, mlancuch, m_dlug_l);
            }
        }

        AsmProxy asmP = new AsmProxy();

        private string fileContent;
        private Stopwatch stopwatch;
        private bool useCSharp = true;

        public Form1()
        {
            InitializeComponent();
            this.Text = "TekstDetektor";
            textBox1.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBoxTime.ReadOnly = true;
            stopwatch = new Stopwatch();
            InitializeComboBox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*";
            openFileDialog1.Title = "Wybierz plik tekstowy";
            textBox1.Multiline = true;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Text = fileContent;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                Task.Run(() =>
                {
                    fileContent = File.ReadAllText(filePath);
                    Invoke(new Action(() => textBox1.Text = fileContent));
                });
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            useCSharp = (comboBoxLanguage.SelectedItem.ToString() == "C#");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fileContent))
            {
                MessageBox.Show("Nie załadowano pliku. Wybierz plik tekstowy przed zliczaniem wystąpień słowa.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string searchedWord = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(searchedWord))
            {
                MessageBox.Show("Wprowadź słowo do zliczenia.");
                return;
            }

            stopwatch.Restart();

            int occurrences;

            if (useCSharp)
            {
                occurrences = CountOccurrences(fileContent, searchedWord);
            }
            else
            {
                occurrences = CountOccurrencesAsm(fileContent, searchedWord);
            }

            stopwatch.Stop();

            textBox3.Text = $"{occurrences}";
            textBoxTime.Text = $"Czas zliczania: {stopwatch.ElapsedMilliseconds} ms";
        }

        private int CountOccurrencesAsm(string content, string searchedWord)
        {
            string linia;
            int dlugosc_linii, do_szukania, dlugosc, jest, znaleziono;
            StreamReader sr = new StreamReader(openFileDialog1.FileName);
            linia = sr.ReadLine();
            znaleziono = 0;
            while (linia != null)
            {
                dlugosc = searchedWord.Length;
                dlugosc_linii = linia.Length;
                do_szukania = 0;
               
                if ((dlugosc_linii - dlugosc) >= 1)
                {
                    do_szukania = dlugosc_linii - dlugosc;
                }
                jest = 0;

                if (do_szukania >= 1)
                {
                    jest = asmP.executeAsmStart(dlugosc, searchedWord, linia, do_szukania);
                }
                if (jest >= 1)
                {
                    znaleziono = znaleziono + jest;
                   
                }

                linia = sr.ReadLine();
            }

            sr.Close();
            return znaleziono;
        }




        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private int CountOccurrences(string content, string searchedWord)
        {
            string linia, podciag;
            int dlugosc_linii, do_szukania, dlugosc, jest, znaleziono, pozycja;
            StreamReader sr = new StreamReader(openFileDialog1.FileName);
            linia = sr.ReadLine();
           
            znaleziono = 0;
            while (linia != null)
            {
                
                dlugosc = searchedWord.Length;
                dlugosc_linii = linia.Length;
                do_szukania = 0;
                
                if ((dlugosc_linii - dlugosc) >= 1)
                {
                    do_szukania = dlugosc_linii - dlugosc;
                }
                jest = 0;

                // tu ASM - czy szukaj = podciag
                if (do_szukania >= 1)
                {
                    pozycja = 0;
                    dlugosc_linii = linia.Length;
                 
                    // to robi to samo co asembler
                    while (pozycja <= (dlugosc_linii - dlugosc))
                    {
                        jest = 0;
                        podciag = linia.Substring(pozycja, dlugosc);

                        // tu ASM - czy szukaj = podciag

                        if (podciag == searchedWord)
                        { 
                        jest = 1;
                        }
                        if (jest == 1)
                        {
                            znaleziono++;
                            
                            pozycja = pozycja + dlugosc ;
                        }
                        else
                            pozycja = pozycja + 1;
                    }

                    
                }
                
               
                linia = sr.ReadLine();
            }


            
            sr.Close();

            return znaleziono;
        }



        private void InitializeComboBox()
        {
            comboBoxLanguage.Items.Add("C#");
            comboBoxLanguage.Items.Add("Asembler");
            comboBoxLanguage.SelectedIndex = 0;

            comboBoxLanguage.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        
    }

  
}