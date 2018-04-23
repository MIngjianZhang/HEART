import java.io.BufferedReader;
  import java.io.InputStreamReader;
  import java.net.Socket;
  import java.io.*;
import java.nio.charset.StandardCharsets;


  public class TcpClientReceiver {


       public static void main(String[] args) {
         String temp;
         float displayFloat;
        try
        {
        //create input stream
        BufferedReader inFromUser = new BufferedReader(new InputStreamReader(System.in));
        //create client socket, connect to server
        Socket clientSocket = new Socket("localhost",50041);
        //create output stream attached to socket
        DataOutputStream outToServer =
                new DataOutputStream(clientSocket.getOutputStream());




        //create input stream attached to socket
        DataInputStream inFromServer = new DataInputStream(clientSocket.getInputStream());


        //send line to server
        //outToServer.writeBytes(temp);

       //read line from server
        //displayBytes = inFromServer.readLine();

        while(true)
        {
	displayFloat = inFromServer.readFloat();
        System.out.println(displayFloat);
        }
        //clientSocket.close();
    }
    catch(Exception ex)
    {

    }
}
}