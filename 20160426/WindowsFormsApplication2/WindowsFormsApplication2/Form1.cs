using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//http://note.chiebukuro.yahoo.co.jp/detail/n1658
//C# で ソケット通信の基礎的サーバー作成(TcpListener + Socket)

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //スレッドからテキストボックスをアクセスすることを指定
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //==========================================
        //メンバー変数

        private System.Net.Sockets.TcpListener server;              //リスナー（接続待ちや受信などを行うｵﾌﾞｼﾞｪｸﾄ）
        private System.Threading.Thread ListeningCallbackThread;    //接続待ちスレッド
        private volatile bool SLTAlive; //接続待ちスレッド終了指示フラグ(volatileが指定されていることに注意)

        //==========================================
        //フォーム起動時イベント

        private void Form1_Load(object sender,EventArgs e)
        {
            this.Text = "ｻｰﾊﾞｰ";    //ﾌｫｰﾑのﾀｲﾄﾙ名
            button1.Text = "ｻｰﾊﾞｰ開始";   //開始ﾎﾞﾀﾝの表示文字
            button2.Text = "ｻｰﾊﾞｰ終了";   //終了ﾎﾞﾀﾝの表示文字
            label1.Text = "";             //状態表示用ﾗﾍﾞﾙを初期化
            //スレッド終了指示フラグを未終了に初期化
            SLTAlive = false;
        }

        //==========================================
        //フォーム閉鎖時イベント

        private void Form1_FormClosed(object sender,FormClosedEventArgs e)
        {
            //ｻｰﾊﾞｱﾌﾟﾘを終了するにもかかわらず、接続待ちｽﾚｯﾄﾞを終了していない場合の処理

            if(SLTAlive)
            {
                //ｽﾚｯﾄﾞ終了指示フラグを終了に設定
                SLTAlive = false;
                //接続要求受け入れの終了
                server.Stop();
                //念のためｽﾚｯﾄﾞをnull設定
                ListeningCallbackThread = null;
            }
        }


        //==========================================
        //接続待ち開始ボタンのクリックイベント


        private void button1_Click(object sender, EventArgs e)
        {
            if(!SLTAlive)   //まだ接続町ｽﾚｯﾄﾞを生成していない場合
            {
                //接続待ち用ｽﾚｯﾄﾞを作成
                ListeningCallbackThread = new System.Threading.Thread(ListeningCallback);
                //接続待ち用ｽﾚｯﾄﾞを開始
                ListeningCallbackThread.Start();
                //ｽﾚｯﾄﾞ終了指示フラグを未終了に設定
                SLTAlive = true;
            }
        }


        //============================================
        //接続待ち終了ボタンのクリックイベント
        private void button2_Click(object sender, EventArgs e)
        {
            if(SLTAlive)    //接続待ちｽﾚｯﾄﾞが作成されていて使える場合
            {
                if(server != null)
                {
                    //接続要求受け入れの終了
                    server.Stop();
                }
                //ｽﾚｯﾄﾞ終了指示フラグを終了に設定
                SLTAlive = false;
                label1.Text = "ｻｰﾊﾞｰ終了";
            }
        }

        //=============================================
        //接続待ちｽﾚｯﾄﾞ用メソッド
        private void ListeningCallback()
        {
            //リスナー（接続要求受け入れ待機）を生成
            //server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), 9000);
            server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse("192.168.1.52"), 9000);

            //*** server = ne

            //接続要求受け入れ開始
            server.Start();
            label1.Text = "ｻｰﾊﾞｰ開始";

            try
            {
                //受信の受付を行うための無限ループ
                while (SLTAlive)         //ｽﾚｯﾄﾞ終了指示フラグでの終了指示がある場合はループ終了
                {
                    //クライアントへの送受信電文操作用のソケット
                    System.Net.Sockets.Socket ClientSocket = server.AcceptSocket(); //Socketクライアント

                    //クライアントからの電文の受信
                    byte[] ReceiveData = new byte[2000];
                    int ResSize = ClientSocket.Receive(ReceiveData, ReceiveData.Length, System.Net.Sockets.SocketFlags.None);      //受信
                    string str = System.Text.Encoding.Unicode.GetString(ReceiveData);
                    textBox1.Text = str;    //受信データ
                    //返信電文をクライアントへ送信
                    byte[] SendBuffer = Encoding.Unicode.GetBytes("本サーバーのご利用ありがとうございます。");
                    int i = ClientSocket.Send(SendBuffer);
                    //Socketクライアントをクローズ
                    ClientSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                    ClientSocket.Close();
                    //短時間だけ待機
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch(Exception ex)
            {
                label1.Text = "サーバ終了";
            }

        }


    }
}
