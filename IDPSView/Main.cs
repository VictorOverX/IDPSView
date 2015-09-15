using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDPSView
{
    public partial class Main : Form
    {

        string check_s;
        string IDPS;

        public Main()
        {
            InitializeComponent();
            verNORExist();
            btnSalvar.Enabled = false;
        }

        /// <summary>
        /// ABRIR O ARQUIVO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbrir_Click(object sender, EventArgs e)
        {
            verNORExist(); // Verifica se as Nor existe
            IDPS = "";
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();

            openFile.Filter         = "BIN Files (.bin)|*.bin|All Files (*.*)|*.*";
            openFile.FilterIndex    = 1;
            openFile.Title          = ("Abrir arquivo dump.bin");
            openFile.FileName       = ("dump.bin");

            txtAbrir.Text           = openFile.FileName;

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {                
                try
                {
                    string FileName = openFile.FileName;

                    using (BinaryReader reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
                    {
                        check_s = "";

                        int length = (int)reader.BaseStream.Length;
                        byte[] myArray = reader.ReadBytes(length);

                        Byte[] metldr;

                        // Verificando se precisa reverter os bytes
                        reader.BaseStream.Position = 0x200;
                        metldr = reader.ReadBytes(3);                        
                        for (int i = 0; i < 3; i++)
                        {
                            check_s += Convert.ToChar(metldr[i]);                            
                        }

                        if (check_s != "IFI")
                        {
                            byte chunk_size = 2;
                            myArray = EndiannessReverse(myArray, chunk_size);
                        }        

                        // Criando o arquivo
                        ByteArrayToFile("dump.bin", myArray);

                        // Lendo IDPS
                        tbIDPS.Text = "";

                        BinaryReader read = new BinaryReader(new FileStream("dump.bin", FileMode.Open));

                        read.BaseStream.Position = 0x2F070;
                        byte[] idps = read.ReadBytes(16);

                        IDPS = ByteArrayToString(idps).ToUpper();
                        tbIDPS.Text = IDPS;


                        btnSalvar.Enabled = true;

                        read.Close();
                        reader.Close();
                    }

                }
                catch
                {
                    MessageBox.Show("Sorry the application seems to have encountered a problem", "Error");
                } 
            }
                      
        }

        /// <summary>
        /// VERIFICA SE NOR EXISTE E APAGA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void verNORExist() 
        {
            if (File.Exists("dump.bin"))
            {
                File.Delete("dump.bin");
            }
        }

        /// <summary>
        /// VERIFICA SE IDPS EXISTE E APAGA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void idpsDelete()
        {
            if (File.Exists("idps.bin"))
            {
                File.Delete("idps.bin");
            }
        }

        /// <summary>
        /// CONVERTER BYTES EM STRING
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


        /// <summary>
        /// CONVERTER EM ARQUIVO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream =
                   new System.IO.FileStream(_FileName, System.IO.FileMode.Create,
                                            System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  _Exception.ToString());
            }

            // error occured, return false
            return false;
        }

        /// <summary>
        /// ENDIANNESS REVERSE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static byte[] EndiannessReverse(byte[] data, int chunk_size)
        {
            byte[] rev = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int chunk_idx = i / chunk_size;
                int byte_pos = (chunk_size - 1) - (i % chunk_size);
                int byte_idx = chunk_idx * chunk_size;
                rev[i] = data[byte_idx + byte_pos];
            }

            return rev;
        }

        /// <summary>
        /// SALVAR IDPS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "BIN Files (.bin)|*.bin|All Files (*.*)|*.*";
                sfd.Title = ("Salvando arquivo IDPS");
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists("idps.bin"))
                    {
                        File.Delete("idps.bin");
                    }

                    File.WriteAllBytes("idps.bin", StringToByteArray(IDPS));
                    File.Move("idps.bin", sfd.FileName);   
                    MessageBox.Show("Arquivo salvo com sucesso!");
                }
            }
        }

        /// <summary>
        /// CLASSE RESPONSAVEL PELA CRIAÇÃO DO ARQUIVO HEXA
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            verNORExist(); // Verifica se as Nor existe
            idpsDelete(); // Verifica se IPDS existe e apaga
        }




    }
}
