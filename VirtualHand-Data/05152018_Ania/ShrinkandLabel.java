import java.io.*;
import java.util.*;

public class ShrinkandLabel {

	private Scanner xcanner;
	
    public static void main(String [] args) {

        // The name of the file to open.
        String fileinputName1 = "EMGSignal1.txt";
        String fileinputName2 = "EMGSignal2.txt";
        String fileinputName3 = "CyberGloveData.txt";
        String fileoutputName = "All.csv";
        String line = "what the fuvckc";
        String[] temp = null;
        String CGData = "Testing";
        Double temp1;
        Double temp0 = 0.0;



        try{
            FileReader fileReader1 = new FileReader(fileinputName1);
            BufferedReader bufferedReader1 = new BufferedReader(fileReader1);
            FileReader fileReader2 = new FileReader(fileinputName2);
            BufferedReader bufferedReader2 = new BufferedReader(fileReader2);
            FileReader fileReader3 = new FileReader(fileinputName3);
            BufferedReader bufferedReader3 = new BufferedReader(fileReader3);
            // Assume default encoding.
            FileWriter fileWriter = new FileWriter(fileoutputName);
            // Always wrap FileWriter in BufferedWriter.
            BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);
            
            String lineEMG1 = bufferedReader1.readLine();        
            String lineEMG2 = bufferedReader2.readLine();
            String lineCyberGlove = bufferedReader3.readLine();
            String sCurrentLine1;
            String sCurrentLine2;
            String sCurrentLine3;

            bufferedWriter.write("EMGSignal1,EMGSignal2,CyberGloveData,Movement");
            bufferedWriter.newLine();

            while((sCurrentLine1 = bufferedReader1.readLine()) != null && (sCurrentLine2 = bufferedReader2.readLine()) != null &&(sCurrentLine3 = bufferedReader3.readLine()) != null){
            	
             // 	if(sCurrentLine.startsWith("5") == true){
             //    	temp = sCurrentLine.split(" ");
             //    	CGData = temp[2];
             //    	System.out.println(sCurrentLine);
             //    	bufferedWriter.write(CGData);
             //    	bufferedWriter.newLine();
            	// } 

                //what I want is first : know the current CyberGlove data and then compare it with the next one, if it's increasing then it's flexion, otherwise extension
                
                
                if (sCurrentLine3 != null){
                    temp1 = Double.parseDouble(sCurrentLine3);
                    System.out.println(temp1);
                    if (temp0 > temp1){
                        System.out.println(temp0 +" is bigger than "+ temp1);
                        bufferedWriter.write(sCurrentLine1 +","+ sCurrentLine2 +","+ sCurrentLine3 +","+"extension");
                        bufferedWriter.newLine();
                    }else if (temp0 == temp1){
                        System.out.println(temp0 +" is equal than "+ temp1);
                        bufferedWriter.write(sCurrentLine1 +","+ sCurrentLine2 +","+ sCurrentLine3 +","+"flexion");
                        bufferedWriter.newLine();
                    }else {
                        System.out.println(temp0 +" is smaller than "+ temp1);
                        bufferedWriter.write(sCurrentLine1 +","+ sCurrentLine2 +","+ sCurrentLine3 +","+"flexion");
                        bufferedWriter.newLine();
                    }
                }
                temp0 = Double.parseDouble(sCurrentLine3);
                System.out.println(temp0);
            }

            /*while(line != null){ 
            	System.out.println(line);
           		
        	}*/            
            bufferedWriter.close();

        }catch(FileNotFoundException ex) {
            System.out.println(
                "Unable to open file '" + 
                fileinputName1 + "'");                
        }
        catch(IOException ex) {
            System.out.println(
                "Error reading file '" 
                + fileoutputName + "'");                  
            // Or we could just do this: 
            // ex.printStackTrace();
        }


    }
}
