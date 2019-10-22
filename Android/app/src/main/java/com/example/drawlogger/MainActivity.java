package com.example.drawlogger;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.PorterDuff;
import android.graphics.PorterDuffXfermode;
import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.opencsv.CSVWriter;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.text.SimpleDateFormat;
import java.util.ArrayList;

public class MainActivity extends AppCompatActivity{

    ImageView chosenImageView;
    Button choosePicture;
    Button erase;
    Button save;
    TextView tv1;
    Canvas canvas;
    Paint paint;
    float downx = 0;
    float downy = 0;
    float upx = 0;
    float upy = 0;

    Bitmap copyImage;
    Bitmap copyImage2;
    Bitmap copyImage3;

    CSVWriter writer;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);
        setContentView(R.layout.activity_main);

        paint = new Paint();
        paint.setColor(Color.rgb(237,0,38));
        paint.setStrokeWidth(5);
        tv1 = (TextView)findViewById(R.id.tv1);


        //csv 데이터
        ArrayList<String[]> csv = new ArrayList<String[]>();
        //csv 헤더 선언
        csv.add(new String[] {"x", "y", "action", "timestamp"});

        String filePath = this.getExternalFilesDir(null).toString() + File.separator + "touchdata.csv";

        FileWriter mFileWriter = null;
        try {
            mFileWriter = new FileWriter(filePath);
            Log.d("HIHI", "FileWriter Creating succeed");
            Log.d("HIHI", "FilePath: " + filePath);
            writer = new CSVWriter(mFileWriter);
        } catch (IOException e) {
            Log.d("HIHI", "FileWriter Creating failed");
        }


        Long tsLong;
        String ts;



        chosenImageView = (ImageView) this.findViewById(R.id.ChosenImageView);
        chosenImageView.setOnTouchListener(new ImageView.OnTouchListener(){
            @Override
            public boolean onTouch(View v, MotionEvent event){
                int action = event.getAction();
                switch (action) {
                    case MotionEvent.ACTION_DOWN:
                        downx = event.getX();
                        downy = event.getY();

                        Long tsLong = System.currentTimeMillis()/1000;
                        String ts = tsLong.toString();
                        csv.add(new String[] {Float.toString(downx), Float.toString(downy), "down", ts});


                        break;
                    case MotionEvent.ACTION_MOVE:
                        upx = event.getX();
                        upy = event.getY();

                        tsLong = System.currentTimeMillis()/1000;
                        ts = tsLong.toString();
                        csv.add(new String[] {Float.toString(upx), Float.toString(upy), "move", ts});


                        canvas.drawLine(downx, downy, upx, upy, paint);
                        chosenImageView.invalidate();
                        downx = upx;
                        downy = upy;
                        break;
                    case MotionEvent.ACTION_UP:
                        upx = event.getX();
                        upy = event.getY();

                        tsLong = System.currentTimeMillis()/1000;
                        ts = tsLong.toString();
                        csv.add(new String[] {Float.toString(upx), Float.toString(upy), "up", ts});


                        canvas.drawLine(downx, downy, upx, upy, paint);
                        chosenImageView.invalidate();
                        break;
                    case MotionEvent.ACTION_CANCEL:
                        break;
                    default:
                        break;
                }
                return true;
            }

        });

        choosePicture = (Button) this.findViewById(R.id.ChoosePictureButton);
        choosePicture.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                if (view == choosePicture) {
                    Intent choosePictureIntent = new Intent(
                            Intent.ACTION_PICK,
                            android.provider.MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
                    startActivityForResult(choosePictureIntent, 0);
                }
            }
        });


        erase = (Button) this.findViewById(R.id.eraseButton);
        erase.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Erase Clicked!");
                Bitmap copyImage3 = copyImage2.copy(Bitmap.Config.ARGB_8888,true);
                canvas = new Canvas(copyImage3);
                int width = 2560;
                int height = 1440;
                chosenImageView.setMinimumWidth(width);
                chosenImageView.setMinimumHeight(height);
                chosenImageView.setMaxWidth(width);
                chosenImageView.setMaxHeight(height);
                chosenImageView.setImageDrawable(new BitmapDrawable(getResources(), copyImage3));
            }
        });

        save = (Button) this.findViewById(R.id.saveButton);
        save.setOnClickListener(new Button.OnClickListener(){
            @Override
            public void onClick(View view){
                Log.d("HIHI","Save Clicked!");

                tv1.setText("Log file is successfully saved");

                try {
                    writer.writeAll(csv);
                    writer.close();
                    Log.d("HIHI", "FileWriter Close succeed");
                } catch (IOException e) {
                    Log.d("HIHI", "FileWriter Close failed");
                }
            }
        });


    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode,
                                    Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode == RESULT_OK) {
            try {
                final Uri imageUri = data.getData();
                final InputStream imageStream = getContentResolver().openInputStream(imageUri);
                final Bitmap selectedImage = BitmapFactory.decodeStream(imageStream);

                copyImage = selectedImage.copy(Bitmap.Config.ARGB_8888,true);
                copyImage2 = selectedImage.copy(Bitmap.Config.ARGB_8888,true);
                canvas = new Canvas(copyImage);
                //canvas.drawBitmap(copyImage,0,0,null);

                int width = 2560;
                int height = 1440;
                chosenImageView.setMinimumWidth(width);
                chosenImageView.setMinimumHeight(height);
                chosenImageView.setMaxWidth(width);
                chosenImageView.setMaxHeight(height);
                chosenImageView.setImageDrawable(new BitmapDrawable(getResources(), copyImage));
            } catch (FileNotFoundException e) {
                e.printStackTrace();
                Toast.makeText(this, "Something went wrong", Toast.LENGTH_LONG).show();
            }
        }else {
            Toast.makeText(this, "You haven't picked Image",Toast.LENGTH_LONG).show();
        }
    }
}
