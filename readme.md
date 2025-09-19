# HebrewAnki
This application generates Hebrew (and Aramaic) flash cards for an application called [Anki](https://apps.ankiweb.net/).

Words and definitions are based on the fantastic work of the [Open Scriptures Hebrew Bible project](https://hb.openscriptures.org), content of chapters is according to their codification of the Westminster Leningrad Codex. Each word in their WLC files is linked via a lemma to an entry in their lexicon for the root of the word found in the WLC. These lexical entries are connected to definitions from the Brown-Driver-Briggs lexicon which are used for the defintions on the answer side of the Anki cards. While the OSHB work is monumental, I believe based on some of its code that much of it was programmatically generated and that not all of it has been fully vetted.

With this app, you'll be able to make a selection of chapters, and it will generate a card for each root Hebrew word found within those chapters. Each word will only be generated once, so if you select the first two chapters of בְּרֵאשִׁ֖ית, the word אֱלֹהִים֙ will appear in chapter 1 but not chapter 2, and as you add more chapters to your collection, it won't be added to any of those either. You'll be prompted to upload your existing collection so that the app can skip any words you've already obtained from any chapter. The benefit of this approach is that it will let you learn all Hebrew you'll need for a specific chapter before reading it as you accumulate more vocabulary, and you won't need to worry about repeats of words for future chapters.

Aramaic words are also optionally included in this collection. Text for prompts for these cards will have a red font color to distinguish from Hebrew cards.

# Disclaimers
This is a project that I initially built purely for personal use and didn't intend for it to be broadly distributed. It's not quite spaghetti code, but it's certainly not robust or well-formed. I consider this project as complete other than any major, breaking bugs that might be found. If there are features you'd like to see, feel free to fork the repository and make them (I just ask that you maintain credit given towards OSHB and AnkiNet, the library my project was built on. I've made a few modifications to AnkiNet code to enable it to do what I needed it to do, but it's given me a huge headstart.

Formatting for the definitions might look a little weird. Many of the answers contain the Hebrew word itself or a different form of it in them which defeats the purpose. Based on the original formatting, there's not a straightforward way to remove these and their surrounding artifacts. So you might see [, ] or something where two words have been removed from the brackets, but the brackets and comma still remain. In answer cards, these words have not been filtered out.

I've quickly added some filtering options that have been lightly tested, but the safest approach is to not change what filters you use once you start a collection. As a rule of thumb, ALWAYS BACK UP YOUR COLLECTION BEFORE MODIFYING IT! There's bound to be some unexpected behavior while using this app. Always keep a backup! Use Anki's built-in backup functionality as well. The application should only ever add cards and never modify existing ones, but caution is always recommended.

The WLC is based on the Septuagint, so some verse numbers may be different than what you might except. All content is still there, but for instance, all of Malachi 4 is contained within Malachi 3, so you won't find a chapter 4 in the selection list.

# How to Get Started
### Starting a new deck
Download the distribution that matches your OS. The application is wrapped in an [Electron](https://github.com/ElectronNET/Electron.NET) shell for portability, so you won't need to install it.

Open the application. For your first use, select No for uploading an existing collection.

Select the filtering options you'd like, then select which chapters of which books you'd like to generate flash cards for.

Click Generate Deck at the bottom of the page, and wait for the Download New Collection button to appear.

Download the collection to a location where you'll be able to import it from your Anki instance.

When importing from this application, turn off the settings "Import any learning progress", "Import any deck presets", and "Merge note types".
Set both "Update notes" and "Update note types" to Never.
Failing to import with these settings may result in lost or corrupted data.

### Adding to an existing deck
To use the application to add more flash cards to your existing deck, you'll need to export the collection from Anki. Use the following settings to do this:
Export format: Anki Deck Package (.apkg)
Include: Hebrew/Aramaic Vocab
Uncheck all checkboxes except for "Support older Anki versions (slower/larger files) - This last one is required! The AnkiNet library dependency I use is not compatible with the newer deck versions, so neither is this application.

Export the file to a location you'll be able to use to upload into the application.

Select Yes to uploading an existing application.

You'll now be able to select (only) new chapters to add to the collection. The resulting collection will only include new words from these new chapters, and importing them into Anki will just add cards without modifying existing ones.

If you decide you want to include more words in chapters you've already generated for, there's an option for that. There's not an option for removing any words from existing chapters, though. You'll have to do that manually through Anki.

## Special Thanks
A special thanks to Aleph with Beth who has made this journey so accessible for us! Also thanks to them for sharing Sang Tian's [article](https://medium.com/@sangsta/how-i-learned-biblical-hebrew-in-a-few-months-471fc8833255) which is what inspired me to try using Anki in the first place!