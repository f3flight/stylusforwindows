package ru.freeflight.testpen;

import android.app.*;
import android.content.*;
import android.os.*;
import android.view.*;
import android.view.View.*;
import android.widget.*;
import android.util.*;
import android.widget.SeekBar.*;
import android.widget.TableRow.*;
import android.transition.*;

public class MainActivity extends Activity 
{
	LinearLayout v, vMargin, vDebug;
	TextView vEvent, vAction, vX, vY, vButtons, vMaxX, vMaxY, vEventCounter, vPressure;
	SeekBar sb;
	Context c;
	long counter = 0L;
	boolean debug = false;
	
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
    }

	@Override
	protected void onResume()
	{
		debug = true;
		if (debug)
		{
			vDebug = (LinearLayout)findViewById(R.id.mainLinearLayoutDebug);
			vDebug.setVisibility(View.VISIBLE);
		}
		
		getWindow().getDecorView().setSystemUiVisibility(
			View.SYSTEM_UI_FLAG_IMMERSIVE
			| View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
			| View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
			| View.SYSTEM_UI_FLAG_FULLSCREEN
    		| View.SYSTEM_UI_FLAG_LAYOUT_STABLE
		);
		
		c = this;
		v = (LinearLayout)findViewById(R.id.mainLinearLayoutMotion);
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
		
		super.onResume();
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
}
