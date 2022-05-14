using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HebResch
{
    public partial class Form1 : Form
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();
            mainProgressBar.Minimum = 0;
            mainProgressBar.Maximum = 100;
        }

        private async void ReadTextFile(CancellationToken cancellationToken)
        {
            var dictWordsCount = new Dictionary<string, int>();
            if (FilePath == "" || FilePath == null)
            {
                MessageBox.Show("Bitte wählen Sie die Datei aus!");
                btnOpenFile_Click(null, null);
            }
            string[] textAll = System.IO.File.ReadAllLines(FilePath);
            decimal intPercentageEachText = 100 / Convert.ToDecimal(textAll.Length);
            decimal I = 0;
            mainProgressBar.Value = 0;
            foreach (string txt in textAll)
            {
                try
                {
                    I = I + intPercentageEachText;
                    cancellationToken.ThrowIfCancellationRequested();
                    string[] arrWords = txt.Split(" ");
                    foreach (string Word in arrWords)
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (dictWordsCount.ContainsKey(Word))
                            {
                                int intOccurence = dictWordsCount[Word];
                                intOccurence = intOccurence + 1;
                                dictWordsCount[Word] = intOccurence;
                            }
                            else
                            {
                                if (Word != "")
                                {
                                    await Task.Run(() => dictWordsCount.Add(Word, 1));
                                }
                            }
                        }
                        catch(OperationCanceledException)
                        {
                            break;
                        }
                    }
                    mainProgressBar.Value = Convert.ToInt32(I);
                }
                catch(OperationCanceledException)
                {
                    break;
                }
            }
            if (cancellationToken.IsCancellationRequested)
            {
                cts = new CancellationTokenSource();
                MessageBox.Show("Der Prozess wurde storniert!");
            }
            else
            {
                DisplayRecords(dictWordsCount, cts.Token);
            }
            
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(cts.IsCancellationRequested)
            {
                cts = new CancellationTokenSource();
            }
            cts.Cancel();
        }

        private void btnReadTextFile_Click(object sender, EventArgs e)
        {
            ReadTextFile(cts.Token);
        }
        private void DisplayRecords(Dictionary<string,int> WordDictionary, CancellationToken cancellationToken)
        {
            grdRecords.Rows.Clear();
            grdRecords.Columns.Clear();
            grdRecords.Columns.Add("colWord", "Wort");
            grdRecords.Columns.Add("colOccurence", "Anzahl");
            foreach (var keyValuePair in WordDictionary)
            {
                try
                {
                    Application.DoEvents();
                    cancellationToken.ThrowIfCancellationRequested();
                    DataGridViewRow row = (DataGridViewRow)grdRecords.Rows[0].Clone();
                    row.Cells[0].Value = keyValuePair.Key;
                    row.Cells[1].Value = keyValuePair.Value;
                    grdRecords.Rows.Add(row);
                }
                catch(OperationCanceledException)
                {
                    MessageBox.Show("Der Prozess 'Tabelle Auflden' wurde storniert");
                    break;
                }

            }
        }

        private string FilePath;
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Text datei auswählen";
            openFileDialog1.DefaultExt = "txt";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FilePath = openFileDialog1.FileName;
                lblFileName.Text = openFileDialog1.SafeFileName;
            }
        }
    }
}
