package ru.f3flight.spendigitizer;

import android.app.*;
import android.content.*;
import android.graphics.*;
import android.net.*;
import android.net.wifi.*;
import android.os.*;
import android.preference.*;
import android.util.*;
import android.view.*;
import android.widget.*;
import com.samsung.samm.common.*;
import com.samsung.spensdk.*;
import com.samsung.spensdk.applistener.*;
import java.io.*;
import java.lang.reflect.*;
import java.net.*;
import java.nio.*;
import android.view.View.*;
import java.util.*;

public class MainActivity extends Activity {
	
	boolean debug = false;
	
	LinearLayout vMotion, vMargin, vDebug;
	TextView vEvent, vAction, vX, vY, vButtons, vMaxX, vMaxY, vEventCounter, vPressure;
	SeekBar sb;
	long counter = 0L;
	
	Context c;
	
	private DatagramSocket socket;
	private byte[] dtab =new byte[21]; //size of data packet
	private DatagramPacket pack;
	private InetAddress broadcastAddress;
	private int counter = 0;
	//private String signalType;
	private int screenWidth, screenHeight;
	private float screenProportions, inputX, inputY;
	
	private String upSignal="";
	private ByteBuffer spenReport;
	private byte SwitchTipState;
	private byte SwitchBarrelState;
	private byte SwitchInvertState;
	private byte SwitchEraserState;
	private byte SwitchInRangeState;
	private byte SwitchFingerState;
	private final byte SwitchTip = 1;
	private final byte SwitchBarrel = 2;
	private final byte SwitchInvert = 4;
	private final byte SwitchEraser = 8;
	private final byte SwitchInRange = 16;
	private final byte SwitchFingerDown = 32;
	private final byte SwitchFingerUp = 64;
	
	int penOnlyTime = 500;
	long penTimerCountDown;
	long penTime;
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		spenReport = ByteBuffer.allocate(dtab.length);
		spenReport.order(ByteOrder.LITTLE_ENDIAN);
//    	requestWindowFeature(Window.FEATURE_NO_TITLE);
		
        StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
        StrictMode.setThreadPolicy(policy); 

        setContentView(R.layout.main);
        
		getWindow().getDecorView().setOnSystemUiVisibilityChangeListener(visChangeListener);
    }
    
    @Override
    protected void onResume() {
        super.onResume();
		
		getWindow().getDecorView().setSystemUiVisibility(
			View.SYSTEM_UI_FLAG_IMMERSIVE
			| View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
			| View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
			| View.SYSTEM_UI_FLAG_FULLSCREEN
    		| View.SYSTEM_UI_FLAG_LAYOUT_STABLE
		);
		
		c = this;
		
        Init();
		
		if (debug)
		{
			vDebug = (LinearLayout)findViewById(R.id.mainLinearLayoutDebug);
			vDebug.setVisibility(View.VISIBLE);
		}
		
		vMotion = (LinearLayout)findViewById(R.id.mainLinearLayoutMotion);
		vMargin = (LinearLayout)findViewById(R.id.mainLinearLayoutMargin);
		
		OnGenericMotionListener ogml = new OnGenericMotionListener()
		{
			@Override
			public boolean onGenericMotion(View p1, MotionEvent p2)
			{
				logMotion("onGenericMotion", p2);
				return true;
			}	
		};
		v.setOnGenericMotionListener(ogml);
		
		OnTouchListener otl = new OnTouchListener()
		{
			@Override
			public boolean onTouch(View p1, MotionEvent p2)
			{
				logMotion("onTouch", p2);
				return true;
			}
		};
		v.setOnTouchListener(otl);
		
		OnHoverListener ohl = new OnHoverListener()
		{
			@Override
			public boolean onHover(View p1, MotionEvent p2)
			{
				logMotion("onHover", p2);
				return true;
			}
		};
		v.setOnHoverListener(ohl);
		
		vX = (TextView)findViewById(R.id.mainTextViewX);
		vY = (TextView)findViewById(R.id.mainTextViewY);
		vMaxX = (TextView)findViewById(R.id.mainTextViewMaxX);
		vMaxY = (TextView)findViewById(R.id.mainTextViewMaxY);
		vEventCounter = (TextView)findViewById(R.id.mainTextViewEventCounter);
		vPressure = (TextView)findViewById(R.id.mainTextViewPressure);
		vEvent = (TextView)findViewById(R.id.mainTextViewEvent);
		vAction = (TextView)findViewById(R.id.mainTextViewAction);
		vButtons = (TextView)findViewById(R.id.mainTextViewButtons);
		
		sb = (SeekBar)findViewById(R.id.mainSeekBar1);
		OnSeekBarChangeListener osbcl = new OnSeekBarChangeListener()
		{

			@Override
			public void onProgressChanged(SeekBar p1, int p2, boolean p3)
			{
				ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams)vMargin.getLayoutParams();
				lp.setMargins(p2,p2,p2,p2);
				vMargin.setLayoutParams(lp);
			}

			@Override
			public void onStartTrackingTouch(SeekBar p1)
			{
				// TODO: Implement this method
			}

			@Override
			public void onStopTrackingTouch(SeekBar p1)
			{
				// TODO: Implement this method
			}	
		};
		sb.setOnSeekBarChangeListener(osbcl);
		screenProportions = 1.0f*vMargin.getWidth()/vMargin.getHeight();
    }
	
	void logMotion(String ListenerName, MotionEvent p2)
	{
		if (debug)
		{
			vEvent.setText(ListenerName);
			vAction.setText(String.valueOf(p2.getActionMasked()));
			vX.setText(String.valueOf(p2.getX()));
			vY.setText(String.valueOf(p2.getY()));
			vButtons.setText(String.valueOf(p2.getButtonState()));
			vMaxX.setText(String.valueOf(vMargin.getWidth()));
			vMaxY.setText(String.valueOf(vMargin.getHeight()));
			vPressure.setText(String.valueOf(p2.getPressure()));
			counter++;
			vEventCounter.setText(String.valueOf(counter));
		}	
	}

	private void setInputXY(float x, float y)
	{
		inputX = x/vMargin.getWidth();
		inputY = y/vMargin.getHeight();
		
	}
	
    private void SendSignal(float x, float y, float pressure, int action, String type)
    {
		try {
	    	//if(signalType!=type)
	    	//{
	    	//	counter=0;
	    	//}
			setInputXY(x, y);
			spenReport.clear();
			spenReport.put((byte)(SwitchTipState+SwitchBarrelState+SwitchInvertState+SwitchEraserState+SwitchInRangeState+SwitchFingerState));
			spenReport.putFloat(inputX);
			spenReport.putFloat(inputY);
			spenReport.putFloat(screenProportions);
			spenReport.putFloat(pressure);
			spenReport.putInt(counter);
			//spenReportX = ByteBuffer.allocate(4).putFloat(x).array();
			//spenReportY = ByteBuffer.allocate(4).putFloat(y).a
			//pack.setData(("|"+Float.toString(x)+"|"+Float.toString(y)+"|"+ Float.toString(pressure)+ "|"+Integer.toString(action)+ "|"+type+ "|"+Integer.toString(counter)+"|"+upSignal).getBytes());
			upSignal = "";
			//socket.send(pack);
			
			pack.setData(spenReport.array());
			socket.send(pack);
			
	    	counter++;
	    	
			//signalType=type;
		} catch (Exception e) {
			e.printStackTrace();
			Toast.makeText(mContext, "error in SendSignal:"+e.getMessage(), Toast.LENGTH_SHORT).show();
		}
    }
    
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }
    
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_exit:
            	finish();	
                break;
            case R.id.menu_clear:
            	mSCanvas.clearSCanvasView();
                break;
            case R.id.menu_settings:
            	Intent settingsActivity = new Intent(mContext,Preferences.class);
            	startActivity(settingsActivity);
                break;
        }
        return true;
    }
    
//    @Override
//    protected void onDestroy() {
//        super.onDestroy();
//        android.os.Process.killProcess(android.os.Process.myPid());
//    }
    
    InetAddress getBroadcastAddress() throws IOException {
        WifiManager wifi = (WifiManager)mContext.getSystemService(WIFI_SERVICE);
        DhcpInfo dhcp = wifi.getDhcpInfo();
		if (dhcp.ipAddress == 0)
		{
			try
			{
				Method method = wifi.getClass().getDeclaredMethod("getWifiApState");
				method.setAccessible(true);
				try
				{
					int actualState = (Integer) method.invoke(wifi, (Object[]) null);
					// Fix for Android 4
					if (actualState>= 10) {
				        actualState -= 10;
					}
					if (actualState == 3)
					{
						Toast.makeText(mContext, "WiFi AP mode", Toast.LENGTH_SHORT).show();
						return InetAddress.getByName("192.168.43.255");
					}
					else
					{
						Toast.makeText(mContext, "WiFi AP state is bad: "+actualState, Toast.LENGTH_SHORT).show();
					}
					
				}
				catch (IllegalArgumentException e)
				{
					Toast.makeText(mContext, "invoke - illegalArgument", Toast.LENGTH_SHORT).show();
				}
				catch (IllegalAccessException e)
				{
					Toast.makeText(mContext, "invoke - illegalAccess", Toast.LENGTH_SHORT).show();
				}
				catch (InvocationTargetException e)
				{
					Toast.makeText(mContext, "invoke - invocationTargetException", Toast.LENGTH_SHORT).show();
				}
			}
			catch (NoSuchMethodException e)
			{
				Toast.makeText(mContext, "getWifiApState - no such method", Toast.LENGTH_SHORT).show();
			}
			
		}
		else
		{
			int broadcast = (dhcp.ipAddress & dhcp.netmask) | ~dhcp.netmask;
			byte[] quads = new byte[4];
			for (int k = 0; k < 4; k++)
				quads[k] = (byte) ((broadcast >> k * 8) & 0xFF);
				Toast.makeText(mContext, "WiFi mode", Toast.LENGTH_SHORT).show();
			return InetAddress.getByAddress(quads);
		}
		Toast.makeText(mContext, "No network detected, please use wifi or hotspot and restart the app", Toast.LENGTH_LONG).show();
		return InetAddress.getLoopbackAddress();
    }
    
    public void Init()
    {
		
    	SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getBaseContext());
    	
        try {
        	
//        	if(prefs.getBoolean("fullscreen", true))
//        	{
//        		this.getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, WindowManager.LayoutParams.FLAG_FULLSCREEN);
//        	}
//        	else
//        	{
//        		this.getWindow().clearFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
//        	}
        	
        	if(socket!=null)
        	{
        		socket.disconnect();
        	}
        	
			socket = new DatagramSocket();
			pack = new DatagramPacket(dtab,dtab.length);
        	
        	if(prefs.getBoolean("autolocate", true))
        	{
	        	broadcastAddress = getBroadcastAddress();
				socket.setBroadcast(true);
        	}
        	else
        	{
            	broadcastAddress =InetAddress.getByName(prefs.getString("host", "192.168.1.101"));
            	socket.setBroadcast(false);
        	}
			
			socket.connect(broadcastAddress, Integer.parseInt(prefs.getString("port", "12333")));
			
		} catch (Exception e) {
			e.printStackTrace();
			Toast.makeText(mContext, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
        
		mSCanvas.setBGColor(Color.BLACK);
		mSCanvas.setSettingStrokeInfo(SObjectStroke.SAMM_STROKE_STYLE_SOLID, 5, Color.DKGRAY);
		mSCanvas.clearSCanvasView();
    }
    
	SCanvasInitializeListener mSCanvasInitializeListener = new SCanvasInitializeListener() {
		public void onInitialized() {
			Init();
			//Toast.makeText(mContext, "Digitizer Ready", Toast.LENGTH_SHORT).show();
		}
	};
	
	SPenHoverListener mSPenHoverListener = new SPenHoverListener(){
		public boolean onHover(View view, MotionEvent event) {
			
			penTimeSet();
			
			SwitchInRangeState = SwitchInRange;
			SwitchTipState = 0;
			SendSignal(event.getX(), event.getY(), event.getPressure(), event.getAction(), "hover");
			
			//if(event.getAction() == MotionEvent.ACTION_UP) {
            //	Toast.makeText(mContext, "UP!", Toast.LENGTH_SHORT).show();
            //} this doesn't happen
			
			return false;
		}

		public void onHoverButtonDown(View view, MotionEvent event) {
		}

		public void onHoverButtonUp(View view, MotionEvent event) {
		}
	};
	
	SPenTouchListener mSPenTouchListener = new SPenTouchListener(){

		public boolean onTouchFinger(View view, MotionEvent event) {
			penTimeUpCheck();
			if (SwitchInRangeState == 0)
			{
				if (event.getAction() == MotionEvent.ACTION_DOWN)
				{
					SwitchFingerState = SwitchFingerDown;
				}
				else if (event.getAction() == MotionEvent.ACTION_UP)
				{
					SwitchFingerState = SwitchFingerUp;
				}
				SendSignal(event.getX(), event.getY(), 0, event.getAction(), "finger");
				SwitchFingerState = 0;
			}
			return true;
		}

		public boolean onTouchPen(View view, MotionEvent event) {
			penTimeSet();
			SwitchInRangeState = SwitchInRange;
			SwitchTipState = SwitchTip;
			SendSignal(event.getX(), event.getY(), event.getPressure(), event.getAction(), "pen");				
			return false;
		}

		public boolean onTouchPenEraser(View view, MotionEvent event) {
			return true;
		}		

		public void onTouchButtonDown(View view, MotionEvent event) {
		}

		public void onTouchButtonUp(View view, MotionEvent event) {
		}			
	};


	public View onCreateView(String name, Context context, AttributeSet attrs) {
		// TODO Auto-generated method stub
		return null;
	}

	public View onCreateView(View parent, String name, Context context,
			AttributeSet attrs) {
		// TODO Auto-generated method stub
		return null;
	}
	
	void penTimeSet()
	{
		penTime = SystemClock.uptimeMillis();
	}
	void penTimeUpCheck()
	{
		if (SwitchInRangeState != 0)
	        if (SystemClock.uptimeMillis() - penTime >= penOnlyTime)
		    {
			    SwitchInRangeState = 0;
				SwitchTipState = 0;
		    }
			
	}
	
	OnSystemUiVisibilityChangeListener visChangeListener = new OnSystemUiVisibilityChangeListener() {

		@Override
		public void onSystemUiVisibilityChange(int p1)
		{
			//Toast.makeText(mContext, "visibility is "+p1, Toast.LENGTH_SHORT).show();
			if (p1 == 0) {
				Timer t = new Timer();
				TimerTask ttask = new TimerTask() {

					@Override
					public void run()
					{
						runOnUiThread(new Runnable(){

							@Override
							public void run()
							{
								getWindow().getDecorView().setSystemUiVisibility(
									View.SYSTEM_UI_FLAG_IMMERSIVE
									| View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
			                        | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
			                        | View.SYSTEM_UI_FLAG_FULLSCREEN
									| View.SYSTEM_UI_FLAG_LAYOUT_STABLE
								);
							}	
						});
					}

				};
				t.schedule(ttask, 10000);
			}
		}
		
	};
	
}
