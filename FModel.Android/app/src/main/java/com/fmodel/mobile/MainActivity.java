package com.fmodel.mobile;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

public class MainActivity extends AppCompatActivity {
    
    private static final int PICK_PAK_FILE = 1;
    private TextView selectedFileText;
    private EditText aesKeyInput;
    private TextView outputPathText;
    private String selectedPakPath = null;
    private File outputDirectory;
    
    private ActivityResultLauncher<String[]> filePickerLauncher;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        // Setup toolbar
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        
        // Initialize views
        selectedFileText = findViewById(R.id.selectedFileText);
        aesKeyInput = findViewById(R.id.aesKeyInput);
        outputPathText = findViewById(R.id.outputPathText);
        Button selectPakButton = findViewById(R.id.selectPakButton);
        Button loadPakButton = findViewById(R.id.loadPakButton);
        Button exportButton = findViewById(R.id.exportButton);
        
        // Setup output directory
        outputDirectory = new File(getFilesDir(), "FModelExports");
        if (!outputDirectory.exists()) {
            outputDirectory.mkdirs();
        }
        outputPathText.setText("Output: " + outputDirectory.getAbsolutePath());
        
        // Setup file picker
        filePickerLauncher = registerForActivityResult(
            new ActivityResultContracts.OpenDocument(),
            uri -> {
                if (uri != null) {
                    handleSelectedFile(uri);
                }
            }
        );
        
        // Setup button listeners
        selectPakButton.setOnClickListener(v -> openFilePicker());
        loadPakButton.setOnClickListener(v -> loadPakFile());
        exportButton.setOnClickListener(v -> exportAssets());
    }
    
    private void openFilePicker() {
        filePickerLauncher.launch(new String[]{"*/*"});
    }
    
    private void handleSelectedFile(Uri uri) {
        selectedPakPath = uri.getPath();
        String fileName = uri.getLastPathSegment();
        selectedFileText.setText("Selected: " + fileName);
        Toast.makeText(this, "File selected: " + fileName, Toast.LENGTH_SHORT).show();
    }
    
    private void loadPakFile() {
        if (selectedPakPath == null) {
            Toast.makeText(this, "Please select a PAK file first", Toast.LENGTH_SHORT).show();
            return;
        }
        
        ProgressDialog progressDialog = new ProgressDialog(this);
        progressDialog.setMessage("Loading PAK file...");
        progressDialog.setCancelable(false);
        progressDialog.show();
        
        new Thread(() -> {
            try {
                // Simulate loading
                Thread.sleep(1000);
                
                runOnUiThread(() -> {
                    progressDialog.dismiss();
                    Toast.makeText(this, "PAK file loaded successfully!", Toast.LENGTH_SHORT).show();
                });
            } catch (Exception e) {
                runOnUiThread(() -> {
                    progressDialog.dismiss();
                    Toast.makeText(this, "Error: " + e.getMessage(), Toast.LENGTH_SHORT).show();
                });
            }
        }).start();
    }
    
    private void exportAssets() {
        if (selectedPakPath == null) {
            Toast.makeText(this, "Please select and load a PAK file first", Toast.LENGTH_SHORT).show();
            return;
        }
        
        ProgressDialog progressDialog = new ProgressDialog(this);
        progressDialog.setMessage("Exporting assets...");
        progressDialog.setProgressStyle(ProgressDialog.STYLE_HORIZONTAL);
        progressDialog.setCancelable(true);
        progressDialog.show();
        
        new Thread(() -> {
            try {
                for (int i = 0; i <= 100; i += 10) {
                    Thread.sleep(500);
                    final int progress = i;
                    runOnUiThread(() -> {
                        progressDialog.setProgress(progress);
                    });
                }
                
                runOnUiThread(() -> {
                    progressDialog.dismiss();
                    Toast.makeText(this, "Export completed!", Toast.LENGTH_LONG).show();
                });
            } catch (Exception e) {
                runOnUiThread(() -> {
                    progressDialog.dismiss();
                    Toast.makeText(this, "Error: " + e.getMessage(), Toast.LENGTH_SHORT).show();
                });
            }
        }).start();
    }
}