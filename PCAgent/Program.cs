using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using ScreenCapturerNS;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mime;

using var are = new AutoResetEvent(false);
byte[]? screenShot = null;
ScreenCapturer.StartCapture(bitmap =>
{
    if (screenShot != null) { return; }
    screenShot = BitmapToByteArray(bitmap);
    File.WriteAllBytes("c:\\temp\\screenshot.png", screenShot);
    ScreenCapturer.StopCapture();
    are.Set();
});

are.WaitOne();
ScreenCapturer.StopCapture();
are.Dispose();
// convert Bitmap to byte[]

var key = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
var agent = new AnthropicClient(key);

var systemMessages = new List<SystemMessage>()
{
    new ("you are expert at image processing, specealized in object recognition"),
};

var messages = new List<Message>
{
    new()
    {
        Role = RoleType.User,
        Content =
        [
            new ImageContent()
            {
                Source = new ImageSource()
                {
                    MediaType = MediaTypeNames.Image.Png,
                    Data = Convert.ToBase64String(screenShot!)
                }
            }
        ]
    },
    new (RoleType.User,
        """
        * analyze the image and write the top and left position in pixels of the taskbar windows logo
        * windows logo is usually at the bottom left of the screen next to search textbox or search icon
        """
        ////* result should be in json format like this:
        ////    {
        ////        "top": 123,
        ////        "left": 456
        ////    }
        ////* if windows logo not found, return this:
        ////    {
        ////        "top": -1,
        ////        "left": -1
        ////    }
        ////* return only json, no other text
        ////"""
        )
};

var parameters = new MessageParameters()
{
    Messages = messages,
    MaxTokens = 1024,
    Model = AnthropicModels.Claude45Haiku,
    Stream = false,
    System = systemMessages,
    Temperature = 1.0m
};

var response = await agent.Messages.GetClaudeMessageAsync(parameters);
Console.WriteLine(response.Content[0]);

static byte[] BitmapToByteArray(Bitmap bitmap)
{
    using MemoryStream ms = new();
    bitmap.Save(ms, ImageFormat.Png); // or ImageFormat.Jpeg, ImageFormat.Bmp, etc.
    return ms.ToArray();
}