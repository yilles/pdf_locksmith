using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PDF_Locksmith
{
    public partial class Form1 : Form
    {
        private int Index = 0;
        private string PDFPath = string.Empty;
        private int LockStatus = -1; //-1-Null, 0-Unlocked, 1-Protected, 2-Locked
        private Form2 fmPassword = new Form2();
        private string Version = "1.0.1";
        public Form1()
        {
            InitializeComponent();
            label4.Text += Version;
            UIUpdate();
        }

        private void UIUpdate()
        {
            switch (LockStatus)
            {
                case 0:
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;
                    label1.Visible = false;
                    label2.Text = Path.GetFileName(PDFPath);
                    label3.Text = "未上鎖, 點擊保護";
                    break;
                case 1:
                    button1.Visible = false;
                    button2.Visible = false;
                    button3.Visible = true;
                    label1.Visible = false;
                    label2.Text = Path.GetFileName(PDFPath);
                    label3.Text = "已保護, 點擊上鎖";
                    break;
                case 2:
                    button1.Visible = false;
                    button2.Visible = true;
                    button3.Visible = false;
                    label1.Visible = false;
                    label2.Text = Path.GetFileName(PDFPath);
                    label3.Text = "已上鎖, 點擊解鎖";
                    break;
                default:
                    button1.Visible = false;
                    button2.Visible = false;
                    button3.Visible = false;
                    label1.Visible = true;
                    label2.Text = string.Empty;
                    label3.Text = string.Empty;
                    break;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            Index = 0;
            PDFPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            UIUpdate();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            // Determine whether string data exists in the drop data. If not, then
            // the drop effect reflects that the drop cannot occur.
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            else
            {
                string pdfPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                using (Stream input = new FileStream(pdfPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    try
                    {
                        PdfReader reader = new PdfReader(input);
                        e.Effect = DragDropEffects.All;
                        try
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                PdfStamper stamper = new PdfStamper(reader, memoryStream);
                                stamper.Close();
                            }
                            LockStatus = 0;
                        }
                        catch (Exception)
                        {
                            LockStatus = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Bad user password")
                        {
                            e.Effect = DragDropEffects.All;
                            LockStatus = 2;
                        }
                    }
                }
            }
        }

        //保護
        private void button1_Click(object sender, EventArgs e)
        {
            string inputPath = string.Empty;
            if (Index == 0)
                inputPath = PDFPath;
            else
                inputPath = string.Format("{0}_unlocked{1}", Path.Combine(Path.GetDirectoryName(PDFPath), Path.GetFileNameWithoutExtension(PDFPath)), Path.GetExtension(PDFPath));
            string outputPath = string.Format("{0}_protected{1}", Path.Combine(Path.GetDirectoryName(PDFPath), Path.GetFileNameWithoutExtension(PDFPath)), Path.GetExtension(PDFPath));
            string password = string.Empty;

            fmPassword.Clear();
            if (fmPassword.ShowDialog() == DialogResult.OK)
                password = fmPassword.Password;
            else
                return;

            try
            {
                using (Stream input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (Stream output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        PdfReader reader = new PdfReader(input);
                        PdfEncryptor.Encrypt(reader, output, true, null, password, PdfWriter.ALLOW_SCREENREADERS);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            LockStatus = 1;
            UIUpdate();
            Index++;
        }

        //解鎖
        private void button2_Click(object sender, EventArgs e)
        {
            string inputPath = string.Empty;
            if (Index == 0)
                inputPath = PDFPath;
            else
                inputPath = string.Format("{0}_locked{1}", Path.Combine(Path.GetDirectoryName(PDFPath), Path.GetFileNameWithoutExtension(PDFPath)), Path.GetExtension(PDFPath));
            string outputPath = string.Format("{0}_unlocked{1}", Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath)), Path.GetExtension(inputPath));
            string password = string.Empty;

            fmPassword.Clear();
            if (fmPassword.ShowDialog() == DialogResult.OK)
                password = fmPassword.Password;
            else
                return;

            try
            {
                PdfReader reader = new PdfReader(inputPath, Encoding.UTF8.GetBytes(password));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PdfStamper stamper = new PdfStamper(reader, memoryStream);
                    stamper.Close();
                    reader.Close();
                    byte[] b = memoryStream.ToArray();
                    using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(b, 0, b.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            LockStatus = 0;
            UIUpdate();
            Index++;
        }

        //上鎖
        private void button3_Click(object sender, EventArgs e)
        {
            string inputPath = string.Empty;
            if (Index == 0)
                inputPath = PDFPath;
            else
                inputPath = string.Format("{0}_protected{1}", Path.Combine(Path.GetDirectoryName(PDFPath), Path.GetFileNameWithoutExtension(PDFPath)), Path.GetExtension(PDFPath));
            string outputPath = string.Format("{0}_locked{1}", Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath)), Path.GetExtension(inputPath));
            string password = string.Empty;

            fmPassword.Clear();
            if (fmPassword.ShowDialog() == DialogResult.OK)
                password = fmPassword.Password;
            else
                return;

            try
            {
                using (Stream output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (PdfReader reader = new PdfReader(inputPath, Encoding.UTF8.GetBytes(password)))
                    {
                        PdfEncryptor.Encrypt(reader, output, true, password, password, PdfWriter.ALLOW_SCREENREADERS);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            LockStatus = 2;
            UIUpdate();
            Index++;
        }
    }
}
