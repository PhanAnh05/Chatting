namespace Chatting.Client.Forms;

public sealed class AvatarPreviewForm : Form
{
    public AvatarPreviewForm(Image image, string title)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(360, 360);
        Size = new Size(640, 640);
        BackColor = Color.FromArgb(15, 23, 42);
        var picture = new PictureBox { Dock = DockStyle.Fill, Image = new Bitmap(image), SizeMode = PictureBoxSizeMode.Zoom, BackColor = BackColor };
        Controls.Add(picture);
    }
}
