namespace Game2
{
	public class InputDlg : Dialog
	{
		protected string[] info;

		public TField tfInput;

		private int padLeft;

		public void show(string info, Command ok, int type)
		{
			tfInput.setText(string.Empty);
			tfInput.setIputType(type);
			this.info = mFont.tahoma_8b.splitFontArray(info, GameCanvas.w - padLeft * 2);
			left = new Command(mResources.CLOSE, GameCanvas.gI(), 8882, null);
			center = ok;
			show();
		}
	}
}
