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
import java.io.*;
import java.lang.reflect.*;
import java.net.*;
import java.nio.*;
import android.view.View.*;
import java.util.*;
import android.widget.SeekBar.*;

public class MainActivity extends Activity {
	
	boolean debug = false;
	int counter = 0;
	
	LinearLayout vMotion, vMargin, vDebug;
	TextView vEvent, vAction, vX, vY, vButtons, vMaxX, vMaxY, vEventCounter, vPressure, vSentX, vSentY, vProportions;
	SeekBar sb;
	
	Context c;
	
	int margin = 0;
	float maxX = 0, maxY = 0, proportions = 0, sentX, sentY;
	
	private DatagramSocket socket;
	private byte[] dtab =new byte[21]; //size of data packet
	private DatagramPacket pack;
	private InetAddress broadcastAddress;
	//private String signalType;
	
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
		
		vMargin = (LinearLayout)findViewById(R.id.mainLinearLayoutMargin);
		
        Init();
		
		vMotion = (LinearLayout)findViewById(R.id.mainLinearLayoutMotion);
		OnGenericMotionListener ogml = new OnGenericMotionListener()
		{
			@Override
			public boolean onGenericMotion(View p1, MotionEvent p2)
			{
				logMotion("onGenericMotion", p2);
				return true;
			}	
		};
		vMotion.setOnGenericMotionListener(ogml);
		OnTouchListener otl = new OnTouchListener()
		{
			@Override
			public boolean onTouch(View p1, MotionEvent p2)
			{
				if (p2.getToolType(0) == MotionEvent.TOOL_TYPE_STYLUS)
				{
					penTimeSet();
					SwitchInRangeState = SwitchInRange;
					SwitchTipState = SwitchTip;
					setButtons(p2.getButtonState());
					SendSignal(p2.getX(), p2.getY(), p2.getPressure(), p2.getAction(), "pen");
				}
				else
				{
					penTimeUpCheck();
					if (SwitchInRangeState == 0)
					{
						if (p2.getAction() == MotionEvent.ACTION_DOWN)
						{
							SwitchFingerState = SwitchFingerDown;
						}
						else if (p2.getAction() == MotionEvent.ACTION_UP)
						{
							SwitchFingerState = SwitchFingerUp;
						}
						SendSignal(p2.getX(), p2.getY(), 0, p2.getAction(), "finger");
						SwitchFingerState = 0;
					}
				}
				logMotion("onTouch", p2);
				return true;
			}
		};
		vMotion.setOnTouchListener(otl);
		OnHoverListener ohl = new OnHoverListener()
		{
			@Override
			public boolean onHover(View p1, MotionEvent p2)
			{
				penTimeSet();

				SwitchInRangeState = SwitchInRange;
				SwitchTipState = 0;
				setButtons(p2.getButtonState());
				SendSignal(p2.getX(), p2.getY(), p2.getPressure(), p2.getAction(), "hover");
				logMotion("onHover", p2);
				return true;
			}
		};
		vMotion.setOnHoverListener(ohl);
		
		vX = (TextView)findViewById(R.id.mainTextViewX);
		vY = (TextView)findViewById(R.id.mainTextViewY);
		vMaxX = (TextView)findViewById(R.id.mainTextViewMaxX);
		vMaxY = (TextView)findViewById(R.id.mainTextViewMaxY);
		vEventCounter = (TextView)findViewById(R.id.mainTextViewEventCounter);
		vPressure = (TextView)findViewById(R.id.mainTextViewPressure);
		vEvent = (TextView)findViewById(R.id.mainTextViewEvent);
		vAction = (TextView)findViewById(R.id.mainTextViewAction);
		vButtons = (TextView)findViewById(R.id.mainTextViewButtons);
		vSentX = (TextView)findViewById(R.id.mainTextViewSentX);
		vSentY = (TextView)findViewById(R.id.mainTextViewSentY);
		vProportions = (TextView)findViewById(R.id.mainTextViewProportions);
		
		sb = (SeekBar)findViewById(R.id.mainSeekBar1);
		OnSeekBarChangeListener osbcl = new OnSeekBarChangeListener()
		{

			@Override
			public void onProgressChanged(SeekBar p1, int p2, boolean p3)
			{
				setMargin(p2);
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
			vMaxX.setText(String.valueOf(maxX));
			vMaxY.setText(String.valueOf(maxY));
			vPressure.setText(String.valueOf(p2.getPressure()));
			vSentX.setText(String.valueOf(sentX));
			vSentY.setText(String.valueOf(sentY));
			vProportions.setText(String.valueOf(maxX / maxY));
			counter++;
			vEventCounter.setText(String.valueOf(counter));
		}	
	}

	void setMargin(int m)
	{
		ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams)vMargin.getLayoutParams();
		lp.setMargins(m,m,m,m);
		vMargin.setLayoutParams(lp);
		margin = m;
	}
	
	private void setInputXY(float x, float y)
	{
		
		sentX = (x-margin)/vMargin.getWidth();
		sentY = (y-margin)/vMargin.getHeight();
		if (sentX < 0) sentX = 0;
		if (sentX > 1) sentX = 1;
		if (sentY < 0) sentY = 0;
		if (sentY > 1) sentY = 1;
		maxX = vMargin.getWidth();
		maxY = vMargin.getHeight();
		proportions = maxX / maxY;
		
	}
	
	void setButtons(int buttonState)
	{
		if (buttonState == 0) SwitchBarrelState = 0;
		if (buttonState == 2) SwitchBarrelState = SwitchBarrel;
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
			spenReport.putFloat(sentX);
			spenReport.putFloat(sentY);
			spenReport.putFloat(proportions);
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
			//Toast.makeText(mContext, "error in SendSignal:"+e.getMessage(), Toast.LENGTH_SHORT).show();
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
            case R.id.menu_settings:
            	Intent settingsActivity = new Intent(c,Preferences.class);
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
        WifiManager wifi = (WifiManager)c.getSystemService(WIFI_SERVICE);
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
						Toast.makeText(c, "WiFi AP mode", Toast.LENGTH_SHORT).show();
						return InetAddress.getByName("192.168.43.255");
					}
					else
					{
						Toast.makeText(c, "WiFi AP state is bad: "+actualState, Toast.LENGTH_SHORT).show();
					}
					
				}
				catch (IllegalArgumentException e)
				{
					Toast.makeText(c, "invoke - illegalArgument", Toast.LENGTH_SHORT).show();
				}
				catch (IllegalAccessException e)
				{
					Toast.makeText(c, "invoke - illegalAccess", Toast.LENGTH_SHORT).show();
				}
				catch (InvocationTargetException e)
				{
					Toast.makeText(c, "invoke - invocationTargetException", Toast.LENGTH_SHORT).show();
				}
			}
			catch (NoSuchMethodException e)
			{
				Toast.makeText(c, "getWifiApState - no such method", Toast.LENGTH_SHORT).show();
			}
			
		}
		else
		{
			int broadcast = (dhcp.ipAddress & dhcp.netmask) | ~dhcp.netmask;
			byte[] quads = new byte[4];
			for (int k = 0; k < 4; k++)
				quads[k] = (byte) ((broadcast >> k * 8) & 0xFF);
				Toast.makeText(c, "WiFi mode", Toast.LENGTH_SHORT).show();
			return InetAddress.getByAddress(quads);
		}
		Toast.makeText(c, "No network detected, please use wifi or hotspot and restart the app", Toast.LENGTH_LONG).show();
		return InetAddress.getLoopbackAddress();
    }
    
    public void Init()
    {
		
    	SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getBaseContext());
	    try
		{
			margin = prefs.getInt("margin",0);
		}
		catch (ClassCastException e)
		{
			Toast.makeText(c, "getInt for margin failed", Toast.LENGTH_SHORT).show();
			Toast.makeText(c, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
    	setMargin(margin);
		
        try
		{
			if (prefs.getBoolean("debug", false))
			{
				debug = true;
				vDebug = (LinearLayout)findViewById(R.id.mainLinearLayoutDebug);
				vDebug.setVisibility(View.VISIBLE);
			}
		}
		catch (ClassCastException e)
		{
			Toast.makeText(c, "getBoolean for debug failed", Toast.LENGTH_SHORT).show();
			Toast.makeText(c, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
			
		try
		{
			
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
			Toast.makeText(c, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
    }
    
	
	
	
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
