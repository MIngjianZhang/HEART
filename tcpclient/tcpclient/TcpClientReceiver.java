import java.io.BufferedReader;
  import java.io.InputStreamReader;
  import java.net.Socket;
  import java.io.*;
import java.nio.charset.StandardCharsets;


  public class TcpClientReceiver {


       public static void main(String[] args) {
         String temp;
         float displayFloat;
         boolean ok = true;
         System.out.println("OKAY / /  ");
        try
        {
        //create input stream
        BufferedReader inFromUser = new BufferedReader(new InputStreamReader(System.in));
        //create client socket, connect to server
        Socket clientSocket = new Socket("localhost",50041);
        System.out.println("OKAY / / / ");
        //create output stream attached to socket
        DataOutputStream outToServer =
                new DataOutputStream(clientSocket.getOutputStream());




        //create input stream attached to socket
        DataInputStream inFromServer = new DataInputStream(clientSocket.getInputStream());

        System.out.println("OKAY / / / / ");
        //send line to server
        //outToServer.writeBytes(temp);

       //read line from server
        //displayBytes = inFromServer.readLine();

        while(ok)
        {
	       displayFloat = inFromServer.readFloat();
        System.out.println(displayFloat);
        System.out.println("OKAY / / /  / // / / ");
        }
        //clientSocket.close();
    }
    catch(Exception ex)
    {

    }
}
}