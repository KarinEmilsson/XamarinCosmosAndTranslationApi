using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using Java.Util;
using Teacher.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(TextToSpeechImplementation))]

namespace Teacher.Droid
{
    public class TextToSpeechImplementation : Java.Lang.Object, ITextToSpeech, TextToSpeech.IOnInitListener
    {
        TextToSpeech speaker;
        string toSpeak;
        string language;

        public void Speak(string text, string languageSet)
        {
            toSpeak = text;
            language = languageSet;
            if (speaker == null)
            {
                speaker = new TextToSpeech(Android.App.Application.Context, this);
            }
            else
            {
                speaker.Speak(toSpeak, QueueMode.Flush, null, null);
            }
        }

        public void OnInit(OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                speaker.SetLanguage(Locale.ForLanguageTag("sv-SE"));
                if ((int)speaker.IsLanguageAvailable(Locale.ForLanguageTag(language)) > 0) speaker.SetLanguage(Locale.ForLanguageTag(language));

                speaker.Speak(toSpeak, QueueMode.Flush, null, null);
            }
        }
    }
}