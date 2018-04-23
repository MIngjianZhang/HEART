import java.util.Scanner;
import java.util.TimerTask;
import java.util.Timer;

public class Counter{
	Timer timer;

    public Reminder(int seconds) {
        timer = new Timer();
        timer.schedule(new RemindTask(), seconds*1000);
	}

    class RemindTask extends TimerTask {
        public void run() {
            System.out.println("Now move to the other direction");
            timer.cancel(); //Terminate the timer thread
        }
    }

    public static void main(String args[]) {
    	System.out.println("Please relax and keep in the middle");
        new Reminder(5);
        System.out.println("Begin scheduled.");
    }
}