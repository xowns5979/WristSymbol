package com.example.drawlogger;
import android.app.ActionBar;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.PorterDuff;
import android.graphics.PorterDuffXfermode;
import android.graphics.Rect;
import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.os.Environment;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.opencsv.CSVWriter;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.net.InetAddress;
import java.net.Socket;
import java.text.SimpleDateFormat;
import java.util.ArrayList;

public class MainActivity extends AppCompatActivity{

    Socket socket;
    DataOutputStream dos;

    int trialNum = 1;

    String[] letterSet = { "a", "c", "f", "j", "l", "r", "t", "v"};

    Button playButton;
    Button enterButton;
    Button clearButton;
    Button finishButton;
    Button nameEnterButton;

    Button highButton;
    Button middleButton;
    Button lowButton;

    TextView tv1;
    TextView tv2;

    Canvas canvas;
    Paint paint;
    float downx = 0;
    float downy = 0;
    float upx = 0;
    float upy = 0;

    EditText name;
    String filePath;
    String globalFilePath;

    int confLevel = -1;

    CSVWriter writer;
    //csv 데이터
    ArrayList<String[]> csv = new ArrayList<String[]>();

    DrawingView dv;
    private Paint mPaint;

    byte[] bytes;
    int[] tactorArr;
    int m1;
    int m2;
    int m3;
    int mode;   // 1 : start pattern

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        int view = R.layout.activity_main;
        super.onCreate(savedInstanceState);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);
        setContentView(view);

        /*
        new Thread() {
            public void run() {
                try {
                    socket = new Socket("143.248.56.106", 5000);
                    dos = new DataOutputStream(socket.getOutputStream());
                    Log.d("HIHI","TCP Connection Succeed!");
                }
                catch(IOException e){
                    e.printStackTrace();
                    Log.d("HIHI","TCP Connection Fail!");
                }
            }
        }.start();
        */
        dv = new DrawingView(this);
        ViewGroup layout = (LinearLayout) findViewById(R.id.drawingLayout);
        dv.setLayoutParams(new ViewGroup.LayoutParams(ActionBar.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT));
        layout.addView(dv);
        //setContentView(dv);

        name = (EditText)findViewById(R.id.nameText);

        mPaint = new Paint();
        mPaint.setAntiAlias(true);
        mPaint.setDither(true);
        mPaint.setColor(Color.GREEN);
        mPaint.setStyle(Paint.Style.STROKE);
        mPaint.setStrokeJoin(Paint.Join.ROUND);
        mPaint.setStrokeCap(Paint.Cap.ROUND);
        mPaint.setStrokeWidth(12);


        tv1 = (TextView)findViewById(R.id.tv1);
        tv2 = (TextView)findViewById(R.id.tv2);


        //csv 헤더 선언
        csv.add(new String[] {"trial","x", "y", "confidence","action","timestamp"});


        Long tsLong;
        String ts;

        globalFilePath =  this.getExternalFilesDir(null).toString();
        filePath = this.getExternalFilesDir(null).toString();

        nameEnterButton = (Button) this.findViewById(R.id.nameEnterButton);
        nameEnterButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","nameEnter Clicked!");

                name.setCursorVisible(false);

                filePath = filePath +  File.separator + name.getText().toString() + "_touchdata.csv";
                FileWriter mFileWriter = null;
                try {
                    mFileWriter = new FileWriter(filePath);
                    Log.d("HIHI", "FileWriter Creating succeed");
                    Log.d("HIHI", "FilePath: " + filePath);
                    writer = new CSVWriter(mFileWriter);
                } catch (IOException e) {
                    Log.d("HIHI", "FileWriter Creating failed");
                }
            }
        });

        lowButton = (Button) this.findViewById(R.id.lowButton);
        lowButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Low Button Clicked!");
                confLevel = 1;
                tv1.setText("LOW");


            }
        });

        middleButton = (Button) this.findViewById(R.id.middleButton);
        middleButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Middle Button Clicked!");
                confLevel = 2;
                tv1.setText("MIDDLE");
            }
        });

        highButton = (Button) this.findViewById(R.id.highButton);
        highButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","High Button Clicked!");
                confLevel = 3;
                tv1.setText("HIGH");
            }
        });


        clearButton = (Button) this.findViewById(R.id.clearButton);
        clearButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Erase Button Clicked!");
                dv.clearCanvas();
            }
        });

        playButton = (Button) this.findViewById(R.id.playButton);
        playButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Start Button Clicked!");

                /*
                edgeWritePattern(letterSet[0]);
                bytes = FourIntToByteArray(tactorArr[0],tactorArr[1],tactorArr[2],1);
                try {
                    dos.write(bytes);
                    //dos.flush();
                }
                catch(IOException e){
                    e.printStackTrace();
                }
                */


            }
        });



        enterButton = (Button) this.findViewById(R.id.enterButton);
        enterButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Enter Button Clicked!");





                File folder = new File(globalFilePath);
                boolean success = false;
                if (!folder.exists())
                {
                    success = folder.mkdirs();
                }

                Log.d("HIHI", globalFilePath);
                File file = new File(globalFilePath + "/sample_"+trialNum+".png");
                if ( !file.exists() )
                {
                    try {
                        success = file.createNewFile();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
                Log.d("HIHI",success+"folder");
                FileOutputStream ostream = null;
                try
                {
                    ostream = new FileOutputStream(file);

                    System.out.println(ostream);
                    View targetView = dv;

                    // myDrawView.setDrawingCacheEnabled(true);
                    //   Bitmap save = Bitmap.createBitmap(myDrawView.getDrawingCache());
                    //   myDrawView.setDrawingCacheEnabled(false);
                    // copy this bitmap otherwise distroying the cache will destroy
                    // the bitmap for the referencing drawable and you'll not
                    // get the captured view
                    //   Bitmap save = b1.copy(Bitmap.Config.ARGB_8888, false);
                    //BitmapDrawable d = new BitmapDrawable(b);
                    //canvasView.setBackgroundDrawable(d);
                    //   myDrawView.destroyDrawingCache();
                    // Bitmap save = myDrawView.getBitmapFromMemCache("0");
                    // myDrawView.setDrawingCacheEnabled(true);
                    //Bitmap save = myDrawView.getDrawingCache(false);
                    Bitmap well = dv.getBitmap();
                    Bitmap save = Bitmap.createBitmap(600, 600, Bitmap.Config.ARGB_8888);
                    Paint paint = new Paint();
                    paint.setColor(Color.WHITE);
                    Paint stroke = new Paint();
                    stroke.setStyle(Paint.Style.STROKE);
                    stroke.setColor(Color.BLACK);
                    stroke.setStrokeWidth(15);
                    Canvas now = new Canvas(save);
                    now.drawRect(new Rect(0,0,600,600), paint);
                    now.drawBitmap(well, new Rect(0,0,well.getWidth(),well.getHeight()), new Rect(0,0,600,600), null);
                    now.drawRect(new Rect(0,0,600,600), stroke);

                    // Canvas now = new Canvas(save);
                    //myDrawView.layout(0, 0, 100, 100);
                    //myDrawView.draw(now);
                    if(save == null) {
                        Log.d("HIHI","NULL bitmap save");

                        System.out.println("NULL bitmap save\n");
                    }
                    save.compress(Bitmap.CompressFormat.PNG, 100, ostream);
                    //bitmap.compress(Bitmap.CompressFormat.PNG, 100, ostream);
                    //ostream.flush();
                    //ostream.close();
                }catch (NullPointerException e)
                {
                    e.printStackTrace();
                    Toast.makeText(getApplicationContext(), "Null error", Toast.LENGTH_SHORT).show();
                }

                catch (FileNotFoundException e)
                {
                    e.printStackTrace();
                    Toast.makeText(getApplicationContext(), "File error", Toast.LENGTH_SHORT).show();
                }

                catch (IOException e)
                {
                    e.printStackTrace();
                    Toast.makeText(getApplicationContext(), "IO error", Toast.LENGTH_SHORT).show();
                }

                dv.clearCanvas();
                trialNum++;
                tv2.setText(String.valueOf(trialNum)+" / 96");
            }
        });

        finishButton = (Button) this.findViewById(R.id.finishButton);
        finishButton.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Finish Button Clicked!");
                try {
                    writer.writeAll(csv);
                    writer.close();
                    Log.d("HIHI", "Log file close succeed");
                } catch (IOException e) {
                    Log.d("HIHI", "Log file close failed");
                }
            }
        });

    }

    public class DrawingView extends View {

        public int width;
        public  int height;
        private Bitmap  mBitmap;
        private Canvas  mCanvas;
        private Path mPath;
        private Paint   mBitmapPaint;
        Context context;
        private Paint circlePaint;
        private Path circlePath;

        public DrawingView(Context c) {
            super(c);
            context=c;
            mPath = new Path();
            mBitmapPaint = new Paint(Paint.DITHER_FLAG);
            circlePaint = new Paint();
            circlePath = new Path();
            circlePaint.setAntiAlias(true);
            circlePaint.setColor(Color.BLUE);
            circlePaint.setStyle(Paint.Style.STROKE);
            circlePaint.setStrokeJoin(Paint.Join.MITER);
            circlePaint.setStrokeWidth(4f);
        }

        @Override
        protected void onSizeChanged(int w, int h, int oldw, int oldh) {
            super.onSizeChanged(w, h, oldw, oldh);
            mBitmap = Bitmap.createBitmap(w, h, Bitmap.Config.ARGB_8888);
            mCanvas = new Canvas(mBitmap);
        }

        @Override
        protected void onDraw(Canvas canvas) {
            super.onDraw(canvas);
            canvas.drawBitmap( mBitmap, 0, 0, mBitmapPaint);
            canvas.drawPath( mPath,  mPaint);
            canvas.drawPath( circlePath,  circlePaint);
        }

        private float mX, mY;
        private static final float TOUCH_TOLERANCE = 4;
        private void clearCanvas() {
            Log.d("HIHI","clearCanvas() reached");
            mCanvas.drawColor(Color.WHITE);
            invalidate();
        }

        private void touch_start(float x, float y) {
            mPath.reset();
            mPath.moveTo(x, y);
            mX = x;
            mY = y;

            Long tsLong = System.currentTimeMillis()/1000;
            String ts = tsLong.toString();
            csv.add(new String[] {String.valueOf(trialNum), Float.toString(x), Float.toString(y),  String.valueOf(confLevel),"down", ts});
        }

        private void touch_move(float x, float y) {
            float dx = Math.abs(x - mX);
            float dy = Math.abs(y - mY);
            if (dx >= TOUCH_TOLERANCE || dy >= TOUCH_TOLERANCE) {
                mPath.quadTo(mX, mY, (x + mX)/2, (y + mY)/2);
                mX = x;
                mY = y;

                circlePath.reset();
                circlePath.addCircle(mX, mY, 30, Path.Direction.CW);

                Long tsLong = System.currentTimeMillis()/1000;
                String ts = tsLong.toString();
                csv.add(new String[] {String.valueOf(trialNum), Float.toString(x), Float.toString(y),  String.valueOf(confLevel),"move", ts});
            }
        }

        private void touch_up(float x, float y) {
            mPath.lineTo(mX, mY);
            circlePath.reset();
            // commit the path to our offscreen
            mCanvas.drawPath(mPath,  mPaint);
            // kill this so we don't double draw
            mPath.reset();

            Long tsLong = System.currentTimeMillis()/1000;
            String ts = tsLong.toString();
            csv.add(new String[] {String.valueOf(trialNum), Float.toString(x), Float.toString(y),  String.valueOf(confLevel),"up", ts});
        }

        @Override
        public boolean onTouchEvent(MotionEvent event) {
            float x = event.getX();
            float y = event.getY();

            switch (event.getAction()) {
                case MotionEvent.ACTION_DOWN:
                    touch_start(x, y);
                    invalidate();
                    break;
                case MotionEvent.ACTION_MOVE:
                    touch_move(x, y);
                    invalidate();
                    break;
                case MotionEvent.ACTION_UP:
                    touch_up(x,y);
                    invalidate();
                    break;
            }
            return true;
        }

        public Bitmap getBitmap()
        {
            //this.measure(100, 100);
            //this.layout(0, 0, 100, 100);
            this.setDrawingCacheEnabled(true);
            this.buildDrawingCache();
            Bitmap bmp = Bitmap.createBitmap(this.getDrawingCache());
            this.setDrawingCacheEnabled(false);


            return bmp;
        }


    }

    public static final byte[] FourIntToByteArray(int m1, int m2, int m3, int mode) {
        return new byte[]{
                (byte) (m1 >>> 24),
                (byte) (m1 >>> 16),
                (byte) (m1 >>> 8),
                (byte) m1,
                (byte) (m2 >>> 24),
                (byte) (m2 >>> 16),
                (byte) (m2 >>> 8),
                (byte) m2,
                (byte) (m3 >>> 24),
                (byte) (m3 >>> 16),
                (byte) (m3 >>> 8),
                (byte) m3,
                (byte) (mode >>> 24),
                (byte) (mode >>> 16),
                (byte) (mode >>> 8),
                (byte) mode,
        };
    }

    public void edgeWritePattern(String character)
    {
        int[] arr = null;
        switch (character.toUpperCase())
        {
            case "A":
                arr = new int[] { 3, 2, 4 };
                break;
            case "C":
                arr = new int[] { 2, 3, 4 };
                break;
            case "F":
                arr = new int[] { 2, 1, 3 };
                break;
            case "L":
                arr = new int[] { 1, 3, 4 };
                break;
            case "J":
                arr = new int[] { 2, 4, 3 };
                break;
            case "R":
                arr = new int[] { 3, 1, 2 };
                break;
            case "T":
                arr = new int[] { 1, 2, 4 };
                break;
            case "V":
                arr = new int[] { 1, 3, 2 };
                break;
        }

        tactorArr = arr;
    }
}
