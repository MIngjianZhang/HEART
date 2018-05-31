import java.io.*;

public class Test {
    public static void main(String [] args) {

        // The name of the file to open.
        String fileinputName = "New Text Document.txt";
        String fileoutputName = "CyberGloveData.txt";
        String line = null;
        String[] temp = null;
        String CGData = "0";

        try{
            FileReader fileReader = new FileReader(fileinputName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);
            line = bufferedReader.readLine();
            if (line.startsWith("5") == true){
                temp = line.split(" ");
                CGData = temp[2];
            }
            // Assume default encoding.
            FileWriter fileWriter = new FileWriter(fileoutputName);

            // Always wrap FileWriter in BufferedWriter.
            BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);

            bufferedWriter.write(CGData);
            bufferedWriter.newLine();
            bufferedWriter.close();

        }catch(FileNotFoundException ex) {
            System.out.println(
                "Unable to open file '" + 
                fileinputName + "'");                
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
