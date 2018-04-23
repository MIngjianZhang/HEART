  import java.io.BufferedReader;
  import java.io.BufferedWriter;
  import java.io.File;
  import java.io.FileWriter;
  import java.io.IOException;
  import java.io.InputStreamReader;
  import java.io.IOException;
  import java.net.Socket;
  import java.io.*;
  import java.lang.*;
  import java.lang.Object;
  import java.nio.file.Files;
  import java.nio.file.Paths;
  import java.nio.file.Path;
  import java.nio.charset.StandardCharsets;
  import java.nio.ByteOrder;
  import java.nio.ByteBuffer;


  public class TcpClientReceiver {


       public static void main(String[] args) {
         String temp;
         float displayFloat;
         int counter = 0;
         int test =0; //Just for debugging purposes
         float temp1 = 0;
         float temp2;
         float originalHandPosition = 0;
         String nowTime = "";
         boolean send = true;
         boolean valid = true;
         File dir = new File ("C://Users//ROC-HCI-1//Downloads//HEART-20170925T165908Z-001//HEART//tmpClient//tmpClient//bin//Debug");
         String[] argument = new String[3];
         String handpositiontosend = "";
         File EMG1 = new File("C:\\Users\\ROC-HCI-1\\Downloads\\Delsys\\tcpclient\\EMGSignal1.txt");
         File EMG2 = new File("C:\\Users\\ROC-HCI-1\\Downloads\\Delsys\\tcpclient\\EMGSignal2.txt");
         File Handpos = new File("C:\\Users\\ROC-HCI-1\\Downloads\\Delsys\\tcpclient\\HanPosition.txt");
         File timelol = new File("C:\\Users\\ROC-HCI-1\\Downloads\\Delsys\\tcpclient\\time.txt");
         argument[0] = "C:\\Users\\ROC-HCI-1\\Downloads\\HEART-20170925T165908Z-001\\HEART\\tmpClient\\tmpClient\\bin\\Debug\\tmpClient.exe";
         Runtime runtime = Runtime.getRuntime();

        try
        {
        //create input stream
        BufferedReader inFromUser = new BufferedReader(new InputStreamReader(System.in));
        //create client socket, connect to server
        Socket clientSocket = new Socket("localhost",50041);
        //create output stream attached to socket
        DataOutputStream outToServer = new DataOutputStream(clientSocket.getOutputStream());
        //create input stream attached to socket
        DataInputStream inFromServer = new DataInputStream(clientSocket.getInputStream());
        //PrintWriter outputStream = new PrintWriter(fileName);
		    FileOutputStream fwEMG1 = new FileOutputStream(EMG1);
        OutputStreamWriter ow1 = new OutputStreamWriter(fwEMG1);
        FileOutputStream fwEMG2 = new FileOutputStream(EMG2);
        OutputStreamWriter ow2 = new OutputStreamWriter(fwEMG2);
        FileOutputStream fwHand = new FileOutputStream(Handpos);
        OutputStreamWriter ow3 = new OutputStreamWriter(fwHand);
        FileOutputStream fwTime = new FileOutputStream(timelol);
        OutputStreamWriter ow4 = new OutputStreamWriter(fwTime);
        BufferedWriter writer1 = new BufferedWriter (ow1);
        BufferedWriter writer2 = new BufferedWriter (ow2);
        BufferedWriter writer3 = new BufferedWriter (ow3);
        BufferedWriter writer4 = new BufferedWriter (ow4);

        //fw1.close();
        ByteBuffer dataBuf = ByteBuffer.allocate(12);
        //send line to server
        //outToServer.writeBytes(temp);

        //read line from server
        //displayBytes = inFromServer.readLine();
        while(true)
        {
	       byte[] tempBytes = new byte[4];
  		   inFromServer.read(tempBytes);
  		   dataBuf.put(tempBytes);
  		   dataBuf.order(ByteOrder.LITTLE_ENDIAN);
  		   displayFloat = dataBuf.getFloat(0);
  		   dataBuf.clear();
  		   dataBuf.order(ByteOrder.BIG_ENDIAN);
		      //displayFloat = inFromServer.readFloat();
         counter++;
         //test++;
         if(valid){
          if(counter % 16 == 0){
            valid = false;
            counter = 0;           
          }
         }else{
          if(counter % 6384 == 0){
        		//manually set the frequency to 2HZ 15984...
            valid = true;
            counter = 0;            
          }
         }
         if(valid){
            if (counter == 1 || counter == 2){
            //This will print stuff like 1,2,160016,160017
            send = !send;
            if (send){
              temp2 = Math.abs(displayFloat);
              argument[2] = Float.toString(temp2);
              temp = (argument[1] + "   "+ argument[2]);
              //System.out.println("The first number is "+temp1 +" and the second one is " + temp2);
              System.out.println(argument[1] + "   "+ argument[2]);
              float currentHandPosition = HandPosition(temp1,temp2,originalHandPosition);
              originalHandPosition = currentHandPosition;
              handpositiontosend = String.valueOf(currentHandPosition);
              System.out.println( tmpresult(temp1, temp2) + "              "+currentHandPosition);
              long nowTi = System.nanoTime();
              nowTime = Long.toString(nowTi/100000000);
              writer1.write(argument[1]);
              writer1.newLine();
              writer1.flush();
              writer2.write(argument[2]);
              writer2.newLine();
              writer2.flush();
              writer3.write(handpositiontosend);
              writer3.newLine();
              writer3.flush();
              writer4.write(nowTime);
              writer4.newLine();
              writer4.flush();
              //System.out.println(nowTime);
              Process process = new ProcessBuilder(argument).start();
              /*try{
              Process p = runtime.exec(new String[]{"tmpClient.exe","0.5","1.0"}, null , dir);
              }catch(IOException e){
                e.printStackTrace();
              }*/
            }
            temp1 = Math.abs(displayFloat);
            argument[1] = Float.toString(temp1);
          }
         }
         
        }
        //clientSocket.close();
    }
    catch(Exception ex)
    {

    }
}
    public static float tmpresult (float a, float b){
      float c;
      c = 400000 *b - 400000* a;
      return c;
    }
    public static float HandPosition (float a, float b, float original){
      float handPosition = 0;
      
      float tmpClient = 400000 *b - 400000* a;

      handPosition = original + tmpClient;
      if (handPosition > 50){
        handPosition = 50;
      } else if (handPosition < -60){
        handPosition = -60;
      }

      return handPosition;
    }
}
