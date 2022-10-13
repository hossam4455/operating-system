using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace os
{
    public class virtual_disk
    {

        public static void initialize()
        {
            if (!File.Exists(@"D:\os\test.txt"))
            {
                StreamWriter wrirtdata = new StreamWriter(@"D:\os\test.txt");
                for (int i = 0; i < 3; i++)
                {

                    if (i == 0)
                    {
                        for (int j = 0; j < 1024; j++)
                            wrirtdata.Write('0');
                    }
                    else if (i == 1)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            for (int j = 0; j < 1024; j++)
                                wrirtdata.Write('*');
                        }
                    }
                    else
                    {

                        for (int k = 0; k < 1019; k++)
                        {
                            for (int j = 0; j < 1024; j++)
                                wrirtdata.Write('#');
                        }

                    }

                }

                wrirtdata.Close();
                FAT.inti();
                Directory Root = new Directory("D:", 0x10, 5, null); //  "Root" Directory
                Root.Read_Directory();
                FAT.Write_Fat_Table();
                Program.current = Root;
                Program.curpath = new string(Program.current.fname);
            }
            else
            {
                FAT.inti();
                Directory Root = new Directory("D:", 0x10, 5, null); // "Root" Directory
                Root.Read_Directory();
                FAT.Write_Fat_Table();
                Program.current = Root;
                Program.curpath = new string(Program.current.fname);
            }

        }
        static public void writeblock(byte[] d, int i)
        {
            FileStream f = new FileStream(@"D:\os\test.txt", FileMode.Open, FileAccess.Write);
            f.Seek(1024 * i, SeekOrigin.Begin);
            f.Write(d, 0, 1024);
            f.Close();
        }

        static public byte[] readblock(int i)
        {
            FileStream f = new FileStream(@"D:\os\test.txt", FileMode.Open, FileAccess.Read);
            f.Seek(1024 * i, SeekOrigin.Begin);
            byte[] r = new byte[1024];
            f.Read(r, 0, 1024);
            f.Close();

            return r;
        }


    }
    class FAT
    {
        static int[] fat_table = new int[1024];

        public static void inti()
        {
            for (int i = 0; i < fat_table.Length; i++)
            {
                if (i < 5) fat_table[i] = -1;
                else fat_table[i] = 0;

            }
        }
        public static void Write_Fat_Table()
        {
            FileStream f1;

            f1 = new FileStream(@"D:\\os\\test.txt", FileMode.Open, FileAccess.Write);
            // StreamWriter ri = new StreamWriter(f1);//to write inside file
            f1.Seek(1024, SeekOrigin.Begin);
            byte[] b = new byte[4096];
            Buffer.BlockCopy(fat_table, 0, b, 0, b.Length);// convert array of int to byute
            f1.Write(b, 0, b.Length);

            f1.Close();
        }
        public static int[] Read_Fat_Table()
        {
            FileStream f1;

            f1 = new FileStream(@"D:\\os\\test.txt", FileMode.Open, FileAccess.Read);
            // StreamWriter ri = new StreamWriter(f1);//to write inside file
            f1.Seek(1024, SeekOrigin.Begin);
            byte[] b = new byte[4096];
            f1.Read(b, 0, b.Length);
            Buffer.BlockCopy(b, 0, fat_table, 0, b.Length);
            return fat_table;
            f1.Close();

        }
        static public int getbvailableblock()
        {
            int i = -1;
            for (i = 5; i < 1024; i++)
            {
                if (fat_table[i] == 0)
                    break;
            }

            return i;
        }

        static public int getavailableblocks()// count ava
        {
            int counter = 0;
            for (int i = 5; i < 1024; i++)
            {
                if (fat_table[i] == 0)
                    counter++;
            }

            return counter;
        }


        static public int getnext(int i)// retuern fat table of i give it index and retuen value
        {
            return fat_table[i];
        }

        static public void setnext(int i, int v)// give it index and change value
        {
            fat_table[i] = v;
        }
        public static int freespace()
        {
            return FAT.getavailableblocks() * 1024; // return free space in the "virtual_disk"
        }



    }

    public class DirectoryEntry
    {
        public char[] fname = new char[11];
        public byte attribute;
        public byte[] f_empty = new byte[12];
        public int firstCluster;// int 
        public int fileSize;// int

        public DirectoryEntry()// emptey contsructor
        {

        }


        public DirectoryEntry(string name, byte dir_attr1, int dir_firstCluster, int fsize = 0)
        {
            fname = name.ToCharArray();
            attribute = dir_attr1;
            firstCluster = dir_firstCluster;
            fileSize = fsize;
        }


        public byte[] get_byt()//cahge var to array of byte and retuern array of byte
        {
            byte[] byts = new byte[32];
            for (int i = 0; i < fname.Length; i++)
            {
                byts[i] = (byte)fname[i];
            }
            byts[11] = attribute;
            for (int i = 0; i < 12; i++)
            {
                byts[i + 12] = f_empty[i];
            }
            byte[] bytfcluster = BitConverter.GetBytes(firstCluster);
            for (int i = 0; i < 4; i++)
            {
                byts[i + 24] = bytfcluster[i];
            }

            byte[] bytes_fsize = BitConverter.GetBytes(fileSize);
            for (int i = 0; i < 4; i++)
            {
                byts[i + 28] = bytes_fsize[i];
            }

            return byts;
        }

        public DirectoryEntry get_directoryentry(byte[] b)
        {
            DirectoryEntry d = new DirectoryEntry();


            for (int i = 0; i < 11; i++)
            {
                d.fname[i] = (char)b[i];
            }

            d.attribute = b[11];


            for (int i = 0; i < 12; i++)
            {
                d.f_empty[i] = 0;
            }

            byte[] d_fcluster = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                d_fcluster[i] = b[i + 24];
            }

            d.firstCluster = BitConverter.ToInt32(d_fcluster, 0);

            byte[] d_filesize = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                d_filesize[i] = b[i + 28];
            }
            d.fileSize = BitConverter.ToInt32(d_filesize, 0); //convert "d_filesize" to int
            return d;
        }

        public DirectoryEntry GetDirectory_Entry()
        {
            DirectoryEntry me = new DirectoryEntry(new string(this.fname), this.attribute, this.firstCluster, this.fileSize);
            return me;
        }
    }


    public class Directory : DirectoryEntry
    {

        public List<DirectoryEntry> Directory_Table;
        public Directory Parent;

        public Directory(string name, byte f_attribute, int f_Cluster, Directory par) : base(name, f_attribute, f_Cluster)
        {
            Directory_Table = new List<DirectoryEntry>();   // create the list "Directory_Table"

            if (par != null) // check if the directory is not the root directory which has not parent
            {
                Parent = par;
            }
        }
        public Directory()
        {

        }

        public void write_directory()
        {

            byte[] Directorytablebytes = new byte[32 * Directory_Table.Count];
            byte[] Directoryentrybytes = new byte[32];


            for (int i = 0; i < Directory_Table.Count; i++)
            {

                Directoryentrybytes = Directory_Table[i].get_byt();

                for (int j = i * 32, c = 0; c < 32; j++, c++)
                {
                    Directorytablebytes[j] = Directoryentrybytes[c];
                }

            }


            decimal requird_blocks = Math.Ceiling((decimal)(Directorytablebytes.Length / 1024));
            int Reminder_Blocks = (Directorytablebytes.Length % 1024);
            int full_blocks = Directorytablebytes.Length / 1024;

            if (requird_blocks <= FAT.getavailableblocks())
            {
                int f_index, Last_index = -1;
                if (firstCluster != 0)
                {
                    f_index = firstCluster;
                }
                else
                {
                    f_index = FAT.getbvailableblock();
                    firstCluster = f_index;
                }
                List<byte[]> blocksdirectory = new List<byte[]>();
                for (int b = 0; b < full_blocks; b++)
                {
                    byte[] blockdirectory = new byte[1024];
                    for (int i = 0; i < 1024; i++)
                    {
                        blockdirectory[i] = Directorytablebytes[(b * 1024) + i];
                    }

                    blocksdirectory.Add(blockdirectory);
                }
                if (Reminder_Blocks > 0)
                {
                    byte[] block_directory = new byte[1024];
                    int st = full_blocks * 1024;
                    for (int i = st; i < (st + Reminder_Blocks); i++)
                    {
                        block_directory[i % 1024] = Directorytablebytes[i];

                    }
                    blocksdirectory.Add(block_directory);
                }

                for (int b = 0; b < blocksdirectory.Count; b++)
                {
                    virtual_disk.writeblock(blocksdirectory[b], f_index);
                    FAT.setnext(f_index, -1);
                    if (Last_index != -1)
                    {
                        FAT.setnext(Last_index, f_index);
                    }
                    Last_index = f_index;
                    f_index = FAT.getbvailableblock();
                }
                FAT.Write_Fat_Table();
            }


        }
        public int searchDirectory(string dirname)
        {
            Read_Directory();
            int position = -1;
            if (dirname.Length < 11)
            {
                for (int i = dirname.Length; i < 11; i++)
                    dirname += "\0";
            }
            else
            {
                dirname = dirname.Substring(0, 11);
            }
            for (int i = 0; i < Directory_Table.Count; i++)
            {
                string directoryname = new string(Directory_Table[i].fname);

                if (directoryname.Equals(dirname))
                {
                    position = i;
                    break;
                }

            }

            return position;
        }

        public void Read_Directory()
        {
            Directory_Table = new List<DirectoryEntry>();
            List<byte> lsread = new List<byte>();
            int fi;
            int next;
            if (firstCluster != 0 && FAT.getnext(firstCluster) != 0)
            {
                fi = firstCluster;
                next = FAT.getnext(fi);

                do
                {
                    lsread.AddRange(virtual_disk.readblock(fi)); //AddRange to add them all
                    fi = next;
                    if (fi != -1)
                    {
                        next = FAT.getnext(fi);
                    }

                } while (next != -1);

                byte[] Bytes_of_Directory = new byte[32];
                for (int i = 0; i < lsread.Count; i++)
                {
                    Bytes_of_Directory[i % 32] = lsread[i];
                    if ((i + 1) % 32 == 0)
                    {
                        DirectoryEntry d = get_directoryentry(Bytes_of_Directory);
                        if (d.fname[0] != '\0')
                            Directory_Table.Add(d);
                    }
                }
            }
        }
        public void UpdateContent(DirectoryEntry dir)
        {
            Read_Directory();
            int index = searchDirectory(new string(dir.fname));

            if (index != -1)
            {
                Directory_Table.RemoveAt(index);

                Directory_Table.Insert(index, dir);
                write_directory();

            }

        }
        public void Delete_Directory()
        {
            if (firstCluster != 0)
            {
                int index = firstCluster;
                int next = FAT.getnext(index);

                do
                {
                    FAT.setnext(index, 0);
                    index = next;
                    if (index != -1)
                    {
                        next = FAT.getnext(index);
                    }

                } while (index != -1);
            }

            if (Parent != null)
            {
                Parent.Read_Directory();
                int index_directory_in_Parent = Parent.searchDirectory(new string(fname));
                if (index_directory_in_Parent != -1)
                {
                    Parent.Directory_Table.RemoveAt(index_directory_in_Parent);
                    Parent.write_directory();
                }
            }

            FAT.Write_Fat_Table();
        }
    }



    class File_Entry : DirectoryEntry
    {
        public Directory Parent;
        public string filecontent;

        public File_Entry(string name, byte directory_attribute, int directory_firstCluster, int f_size, Directory par, string Content) : base(name, directory_attribute, directory_firstCluster, f_size)
        {
            filecontent = Content;
            if (par != null)
                Parent = par;
        }

        public void Writefilecontent()
        {
            byte[] bytescontent = Encoding.ASCII.GetBytes(filecontent);
            double Requird_Blocks = Math.Ceiling((bytescontent.Length / 1024.0));
            int Reminder_Blocks = (bytescontent.Length % 1024);
            int full_blocks = bytescontent.Length / 1024;

            if (Requird_Blocks <= FAT.getbvailableblock())
            {
                int f_index;
                int Last_index = -1;
                if (firstCluster != 0)
                {
                    f_index = firstCluster;
                }
                else
                {
                    f_index = FAT.getbvailableblock();
                    firstCluster = f_index;
                }

                List<byte[]> blocksFContent = new List<byte[]>();


                for (int b = 0, j = b * 1024; b < full_blocks; b++)// loop to fill the list "blocksFContent"
                {
                    byte[] blockcontent = new byte[1024];
                    for (int i = 0; i < 1024; i++, j++)
                    {
                        blockcontent[i] = bytescontent[(b * 1024) + i];
                    }

                    blocksFContent.Add(blockcontent); // add the completed block to the list "blocksFContent"

                }
                if (Reminder_Blocks > 0)
                {
                    byte[] blockdirectory = new byte[1024];
                    int st = full_blocks * 1024;
                    for (int i = st; i < (st + Reminder_Blocks); i++)
                    {
                        blockdirectory[i % 1024] = bytescontent[i];

                    }
                    blocksFContent.Add(blockdirectory);
                }

                for (int b = 0; b < Requird_Blocks; b++)
                {
                    virtual_disk.writeblock(blocksFContent[b], f_index);
                    FAT.setnext(f_index, -1);
                    if (Last_index != -1)
                    {
                        FAT.setnext(Last_index, f_index);
                    }
                    Last_index = f_index;
                    f_index = FAT.getbvailableblock();

                }

                FAT.Write_Fat_Table();
            }

        }

        public void Readfilecontent()
        {
            if (firstCluster != 0 && FAT.getnext(firstCluster) != 0)
            {
                int fi = firstCluster;
                int next = FAT.getnext(fi);
                List<byte> ls = new List<byte>();
                do
                {
                    ls.AddRange(virtual_disk.readblock(fi));
                    fi = next;
                    if (fi != -1)
                    {
                        next = FAT.getnext(fi);
                    }

                } while (next != -1);
                filecontent = Encoding.ASCII.GetString(ls.ToArray());
            }
        }

        public void Deletefilecontent()
        {
            if (firstCluster != 0)
            {
                int index = firstCluster;
                int next = FAT.getnext(index);

                do
                {
                    FAT.setnext(index, 0);
                    index = next;
                    if (index != -1)
                    {
                        next = FAT.getnext(index);
                    }

                } while (index != -1);
            }
            if (Parent != null)
            {
                Parent.Read_Directory();
                int index_in_Parent = Parent.searchDirectory(new string(fname));
                if (index_in_Parent != -1) // if exist
                {
                    Parent.Directory_Table.RemoveAt(index_in_Parent);
                    Parent.write_directory();
                }
            }
            FAT.Write_Fat_Table();
        }

    }

    class Commands
    {
        public static void md(string dir_name)
        {
            int index = Program.current.searchDirectory(dir_name);

            if (index == -1)
            {
                DirectoryEntry directory = new DirectoryEntry(dir_name, 0x10, 0);
                Program.current.Directory_Table.Add(directory);
                Program.current.write_directory();
                if (Program.current.Parent != null)
                {
                    Program.current.Parent.UpdateContent(Program.current.GetDirectory_Entry());
                    Program.current.Parent.write_directory();
                }
                FAT.Write_Fat_Table();
            }
            else
            {
                Console.WriteLine($"That Directory already exists");
            }
        }

        public static void rd(string dir_name)
        {
            int index = Program.current.searchDirectory(dir_name);

            if (index != -1)
            {
                int fcluster = Program.current.Directory_Table[index].firstCluster;
                Directory directory = new Directory(dir_name, 0x10, fcluster, Program.current);
                directory.Delete_Directory();
            }
            else
            {
                Console.WriteLine($"The system cannot find the file specified");
            }
        }

        public static void cd(string dir_name)
        {
            int index = Program.current.searchDirectory(dir_name);
            if (index != -1) // if exists
            {
                byte attribute = Program.current.Directory_Table[index].attribute;//........
                if (attribute == 0x10)
                {
                    int F_cluster = Program.current.Directory_Table[index].firstCluster;
                    Directory directory = new Directory(dir_name, 0x10, F_cluster, Program.current);
                    Program.current = directory;
                    string p = Program.curpath;
                    Program.curpath = p + "\\" + dir_name;
                    Program.current.Read_Directory();
                }
            }
            else
            {
                Console.WriteLine($"The system cannot find the file specified ");
            }

        }

        public static void dir()
        {
            Program.current.Read_Directory();
            int fcounter = 0;
            int dcounter = 0;
            int Size_Files = 0;
            Console.WriteLine($" Directory of : {new string(Program.current.fname)}");

            for (int i = 0; i < Program.current.Directory_Table.Count; i++)
            {
                if (Program.current.Directory_Table[i].attribute == 0x0)
                {
                    Console.WriteLine("      " + Program.current.Directory_Table[i].fileSize + "    " + new string(Program.current.Directory_Table[i].fname));
                    fcounter++;
                    Size_Files += Program.current.Directory_Table[i].fileSize;
                }
                else
                {
                    Console.WriteLine("{0}{1:11}", "\t<DIR>    ", new string(Program.current.Directory_Table[i].fname));
                    dcounter++;
                }
            }
            Console.WriteLine();
            Console.WriteLine($"{"          "}{fcounter} File(s)   {Size_Files } bytes");
            Console.WriteLine($"{"          "}{dcounter} Dir(s)   {FAT.freespace()} bytes free");

        }

        public static void import(string path) //  import file from computer disk to the virtual disk
        {
            if (File.Exists(path))
            {
                string name = Path.GetFileName(path);
                string content = File.ReadAllText(path);
                int size = content.Length;
                int index = Program.current.searchDirectory(name);
                if (index == -1)
                {
                    int fcluster = 0;
                    if (size > 0)
                    {
                        fcluster = FAT.getbvailableblock();
                    }
                    File_Entry f_entry = new File_Entry(name, 0x0, fcluster, size, Program.current, content);
                    f_entry.Writefilecontent();
                    DirectoryEntry d_entry = new DirectoryEntry(name, 0x0, fcluster, size);
                    Program.current.Directory_Table.Add(d_entry);
                    Program.current.write_directory();
                }
                else
                {
                    Console.WriteLine($"directory already exists");
                }
            }
            else
            {
                Console.WriteLine("The system cannot find the file specified");
            }

        }

        public static void type(string f_name) // print the content of file
        {
            int index = Program.current.searchDirectory(f_name);
            if (index != -1) //the file exists 
            {
                int f_cluster = Program.current.Directory_Table[index].firstCluster;
                int f_size = Program.current.Directory_Table[index].fileSize;
                string content = null;
                File_Entry fentry = new File_Entry(f_name, 0x0, f_cluster, f_size, Program.current, content);
                fentry.Readfilecontent();
                Console.WriteLine(fentry.filecontent);
            }
            else
            {
                Console.WriteLine("The system cannot find the file specified");
            }
        }

        public static void export(string source, string destination)
        {
            int index = Program.current.searchDirectory(source);
            if (index != -1)
            {
                if (System.IO.Directory.Exists(destination))
                {
                    int f_cluster = Program.current.Directory_Table[index].firstCluster;
                    int f_size = Program.current.Directory_Table[index].fileSize;
                    string content = null;
                    File_Entry f_entry = new File_Entry(source, 0x0, f_cluster, f_size, Program.current, content);
                    f_entry.Readfilecontent();

                    StreamWriter sw = new StreamWriter(destination + "\\" + source);
                    sw.Write(f_entry.filecontent);
                    sw.Flush();
                    sw.Close();
                }
                else
                {
                    Console.WriteLine("The system cannot find the specified path on computer disk");
                }

            }
            else
            {
                Console.WriteLine("The system cannot find the file specified");
            }

        }

        public static void help()
        {

            Console.WriteLine("cd -  Change the current default directory to .\n     If the argument is not present, report the current directory.\n     If the directory does not exist an appropriate error should be reported");
            Console.WriteLine("'cls' To Clear console");
            Console.WriteLine("'del' To delete Files");
            Console.WriteLine("'md' To Make directory");
            Console.WriteLine("'rd' To Remaove Directory");
            Console.WriteLine("'rename' To Rename File or Directory");
            Console.WriteLine("'type' To Show Text File");
            Console.WriteLine("dir To Show Content The Directory");
            Console.WriteLine("quit To Exit From Console (cmd)");
            Console.WriteLine("import – import text file(s) from your computer");
            Console.WriteLine("export – export text file(s) to your computer");
        }
        public static void del(string file_name) //  delete files
        {
            int index = Program.current.searchDirectory(file_name);
            if (index != -1)
            {
                if (Program.current.Directory_Table[index].attribute == 0x0)
                {
                    int f_cluster = Program.current.Directory_Table[index].firstCluster;
                    int f_size = Program.current.Directory_Table[index].fileSize;
                    File_Entry fentry = new File_Entry(file_name, 0x0, f_cluster, f_size, Program.current, null);
                    fentry.Deletefilecontent();
                }
                else
                {
                    Console.WriteLine("The system cannot find the file specified");
                }
            }
            else
            {
                Console.WriteLine("The system cannot find the file specified");
            }
        }

        public static void rename(string old_name, string new_name)// rename file or directory
        {
            int index_old = Program.current.searchDirectory(old_name);
            if (index_old != -1)
            {
                int index_new = Program.current.searchDirectory(new_name);
                if (index_new == -1)
                {
                    DirectoryEntry d_entry = Program.current.Directory_Table[index_old];
                    d_entry.fname = new_name.ToCharArray();
                    Program.current.Directory_Table.RemoveAt(index_old);
                    Program.current.Directory_Table.Insert(index_old, d_entry);
                    Program.current.write_directory();
                }
                else
                {
                    Console.WriteLine("Duplicate file name exists");
                }

            }
            else
            {
                Console.WriteLine("The system cannot find the file or folder specified");
            }

        }
    }



    class Program
    {


        public static Directory current = new Directory();
        public static string curpath = "";
        static void Main(string[] args)
        {
            virtual_disk.initialize();
            curpath = new string(current.fname);
            curpath = curpath.Trim(new char[] { '\0', ' ' });
            while (true)
            {
                Console.Write(curpath + "\\" + ">>");
                string input;
                input = Console.ReadLine();
                string[] Split = input.Split(' ');
                if (Split.Length == 1)
                {
                    if (Split[0].ToLower() == "cls") { Console.Clear(); }
                    else if (Split[0].ToLower() == "quit") { Environment.Exit(0); }
                    else if (Split[0].ToLower() == "help") { Commands.help(); }
                    else if (Split[0].ToLower() == "dir") { Commands.dir(); }
                    else { Console.WriteLine("Command is Not Found"); }

                }
                else if (Split.Length == 2)
                {
                    if (Split[0].ToLower() == "md") { Commands.md(Split[1]); }
                    else if (Split[0].ToLower() == "cd") { Commands.cd(Split[1]); }
                    else if (Split[0].ToLower() == "rd") { Commands.rd(Split[1]); }
                    else if (Split[0].ToLower() == "import") { Commands.import(Split[1]); }
                    else if (Split[0].ToLower() == "type") { Commands.type(Split[1]); }
                    else if (Split[0].ToLower() == "del") { Commands.del(Split[1]); }
                    else { Console.WriteLine("Command is Not Found"); }

                }
                else if (Split.Length == 3)
                {
                    if (Split[0].ToLower() == "export") { Commands.export(Split[1], Split[2]); }
                    else if (Split[0].ToLower() == "rename") { Commands.rename(Split[1], Split[2]); }
                    else { Console.WriteLine("Command is Not Found"); }

                }
                else
                {
                    Console.WriteLine("Command is Not Found");
                }
            }
        }


    }
}