# Configuration Samples

Here I collect some real-world samples of [configuration](overview.md#configuration).

## Exporting Text Items in Plain Text

This sample configuration exports a set of Cadmus text items (ignoring any layers) into a set of plain text files, to allow third party tools further process the resulting text.

Here the sample tool is a Chiron-based linguistic analyzer for prose rhythm, having as input a set of plain text files with the text to be analyzed. This text is preceded by a metadata header, where each metadatum is in a single line starting with `.` and having a name followed by `=` and its value. For instance, this is a document from _Constitutiones Sirmondianae_:

```txt
.date=333 Mai. 5
.date-value=333
.data=Dat. III nonas Maias Constantinopoli Dalmatio et Zenofilo conss.
.title=01 Imp(erator) Constantinus A(ugustus) ad Ablabium pp. 
Satis mīrātī sumus gravitātem tuam, quae plēna iūstitiae ac probae religiōnis est, clēmentiam nostram scīscitārī voluisse, quid dē sententiīs epīscopōrum vel ante moderātiō nostra cēnsuerit vel nunc servārī cupiāmus, Ablābī, parēns kārissime atque amantissime. Itaque quia ā nōbīs īnstruī voluistī, ōlim prōmulgātae lēgis ōrdinem salūbrī rūrsus imperiō propāgāmus. Sānximus namque, sīcut ēdictī nostrī fōrma dēclārat, sententiās epīscopōrum quōlibet genere lātās sine aliquā aetātis discrētiōne inviolātās semper incorruptāsque servārī; scīlicet ut prō sānctīs semper ac venerābilibus habeantur, quidquid epīscopōrum fuerit sententiā terminātum. Sīve itaque inter minōrēs sīve inter maiōrēs ab epīscopīs fuerit iūdicātum, apud vōs, quī iūdiciōrum summam tenētis, et apud cēterōs omnēs iūdicēs ad exsecūtiōnem volumus pertinēre. Quīcumque itaque lītem habēns, sīve possessor sīve petītor vel inter initia lītis vel dēcursīs temporum curriculīs, sīve cum negōtium perōrātur, sīve cum iam coeperit prōmī sententia, iūdicium ēlēgerit sacrōsānctae lēgis antistitis, īlicō sine aliquā dubitātiōne, etiamsī alia pars refrāgātur, ad epīscopum persōnae lītigantium dīrigantur. Multa enim, quae in iūdiciō captiōsa praescrīptiōnis vincula prōmī nōn patiuntur, investīgat et pūblicat sacrōsānctae religiōnis auctōritās. Omnēs itaque causae, quae vel praetōriō iūre vel cīvīlī tractantur, epīscopōrum sententiīs terminātae perpetuō stabilitātis iūre firmentur, nec liceat ulterius retractārī negōtium, quod epīscopōrum sententia dēciderit. Testimōnium etiam ab ūnō licet epīscopō perhibitum omnis iūdex indubitanter accipiat nec alius audiātur testis, cum testimōnium epīscopī ā quālibet parte fuerit reprōmissum. Illud est enim vēritātis auctōritāte firmātum, illud incorruptum, quod ā sacrōsānctō homine cōnscientia mentis inlībātae prōtulerit. Hoc nōs ēdictō salūbrī aliquandō cēnsuimus, hoc perpetuā lēge firmāmus, malitiōsa lītium sēmina comprimentēs, ut miserī hominēs longīs ac paene perpetuīs āctiōnum laqueīs implicātī ab improbīs petītiōnibus vel ā cupiditāte praeposterā mātūrō fīne discēdant. Quidquid itaque dē sententiīs epīscopōrum clēmentia nostra cēnsuerat et iam hāc sumus lēge conplexī, gravitātem tuam et cēterōs prō ūtilitāte omnium lātum in perpetuum observāre convēnit.
```

In our scenario, Cadmus text items have a facet equal to `text`, and use a `TokenTextPart` for the text. They also use layer parts, like critical apparatus; but here we are just interested in exporting the raw text. As it stands in Cadmus, the text is just a set of items, where each part contains a paragraph or a poetical composition cited in the context of a document. This ensures that every item stands on its own, and can get the required layers. These text portions are virtually grouped under each "work" by means of item group IDs.

The text being edited in Cadmus in this sample is Sidonius Apollinaris letters. Their text is split into items at each paragraph or poetical composition, and each chunk of text belongs to a letter via its group ID, which has the form `N-NNN` where `N` is the book number (1-9) and `NNN` is the letter number in that book. For instance, `1-002` is the second letter of the first book.

We want to extract the raw text from each of these chunks, in their order, and create a new file for each letter. Also, we want some preprocessing. For instance, many letters end with the salutation `vale`, like e.g. 1.1:

```txt
... sed si et hisce deliramentis genuinum molarem invidia non fixerit, actutum tibi a nobis volumina numerosiora percopiosis scaturrientia sermocinationibus multiplicabuntur. vale.
```

As we are going to analyze prose rhythm, such salutations would introduce rumor in our analysis data. So, we want to remove them during export. This is the configuration used:

```json
{
  "RendererFilters": [
    {
      "Keys": "rep-filter",
      "Id": "it.vedph.renderer-filter.replace",
      "Options": {
        "Replacements": [
          {
            "Source": "([.;:?!])\\s+vale\\.[ ]*([\\r\\n]+)",
            "IsPattern": true,
            "Target": "$1$2",
            "Repetitions": 1
          },
          {
            "Source": "\\d+\\.\\s+",
            "IsPattern": true,
            "Target": "",
            "Repetitions": 1
          }
        ]
      }
    },
    {
      "Keys": "split-filter",
      "Id": "it.vedph.renderer-filter.sentence-split",
      "Options": {
        "EndMarkers": ".?!",
        "Trimming": true
      }
    }
  ],
  "TextPartFlatteners": [
    {
      "Keys": "it.vedph.token-text",
      "Id": "it.vedph.text-flattener.token"
    }
  ],
  "TextBlockRenderers": [
    {
      "Keys": "txt",
      "Id": "it.vedph.text-block-renderer.txt",
      "Options": {
        "FilterKeys": ["rep-filter", "split-filter"]
      }
    }
  ],
  "ItemComposers": [
    {
      "Keys": "default",
      "Id": "it.vedph.item-composer.txt.fs",
      "Options": {
        "TextPartFlattenerKey": "it.vedph.token-text",
        "TextBlockRendererKey": "txt",
        "ItemGrouping": true,
        "OutputDirectory": "c:\\users\\dfusi\\Desktop\\out",
        "TextHead": ".author=Sidonius Apollinaris\r\n.date=v AD\r\n.date-value=450\r\n.title={item-title}\r\n"
      }
    }
  ],
  "ItemIdCollector": {
    "Id": "it.vedph.item-id-collector.mongo",
    "Options": {
      "FacetId": "text"
    }
  }
}
```

1. a replacer renderer filter is used to remove the final `vale` and eventual artifacts represented by paragraph numbers. To this end, we use a couple of regular expressions. This filter is defined with key `rep-filter`.
2. a sentence splitting filter is used to rearrange newlines so that each line corresponds to a sentence. This facilitates the usage of the target tool.
3. a text part flattener is used to flatten the token-based text part of each text item. This part's model has a list of lines, each with its text. These lines will become rows of text blocks; in this case, given that we include no layer in the output, we will just have a single block for each row.
4. a text block renderer is used to extract blocks as plain text. Also, once extracted the text gets filtered by the `rep-filter` defined above.
5. an item composer puts all these pieces together: it is a plain text, file-based composer, using the text flattener and block renderer defined above; it applies grouping, i.e. it will change its output file whenever a new group is found; uses the specified output directory, and prepends to each file a "header" with the format explained above. This header includes metadata placeholders between curly braces. For instance, `{item-title}` will be replaced by the title of each item being processed. File names instead will be equal to group IDs.
6. an item ID collector is used to collect all the text items (facet ID = `text`) from the MongoDB database containing Sidonius Apollinaris.

The command used in the CLI is (assuming that this configuration file is named `Preview-txt` under my desktop):

```ps1
./cadmus-mig render-items cadmus-sidon C:\Users\dfusi\Desktop\Preview-txt.json
```

The first file output by this configuration, without the sentence splitting filter, would be:

```txt
.author=Sidonius Apollinaris
.date=v AD
.date-value=450
.title=1_001_001 Sidonius Constantio suo salutem.
Diu praecipis, domine maior, summa suadendi auctoritate, sicuti es in his quae deliberabuntur consiliosissimus, ut, si quae litterae paulo politiores varia occasione fluxerunt, prout eas causa persona tempus elicuit, omnes retractatis exemplaribus enucleatisque uno volumine includam, Quinti Symmachi rotunditatem, Gai Plinii disciplinam maturitatemque vestigiis praesumptiosis insecuturus.
nam de Marco Tullio silere melius puto, quem in stilo epistulari nec Iulius Titianus sub nominibus illustrium feminarum digna similitudine expressit. propter quod illum ceteri quique Frontonianorum utpote consectaneum aemulati, cur veternosum dicendi genus imitaretur, oratorum simiam nuncupaverunt. quibus omnibus ego immane dictu est quantum semper iudicio meo cesserim quantumque servandam singulis pronuntiaverim temporum suorum meritorumque praerogativam.
sed scilicet tibi parui tuaeque examinationi has <litterulas> non recensendas (hoc enim parum est) sed defaecandas, ut aiunt, limandasque commisi, sciens te immodicum esse fautorem non studiorum modo verum etiam studiosorum. quam ob rem nos nunc perquam haesitabundos in hoc deinceps famae pelagus impellis.
porro autem super huiusmodi opusculo tutius conticueramus, contenti versuum felicius quam peritius editorum opinione, de qua mihi iampridem in portu iudicii publici post lividorum latratuum Scyllas enavigatas sufficientis gloriae ancora sedet. sed si et hisce deliramentis genuinum molarem invidia non fixerit, actutum tibi a nobis volumina numerosiora percopiosis scaturrientia sermocinationibus multiplicabuntur.
```

Note that here the original letter had a final `vale.` which has been removed by the filter.

By applying also sentence splitting, the result is:

```txt
.author=Sidonius Apollinaris
.date=v AD
.date-value=450
.title=1_001_001 Sidonius Constantio suo salutem.
Diu praecipis, domine maior, summa suadendi auctoritate, sicuti es in his quae deliberabuntur consiliosissimus, ut, si quae litterae paulo politiores varia occasione fluxerunt, prout eas causa persona tempus elicuit, omnes retractatis exemplaribus enucleatisque uno volumine includam, Quinti Symmachi rotunditatem, Gai Plinii disciplinam maturitatemque vestigiis praesumptiosis insecuturus.
nam de Marco Tullio silere melius puto, quem in stilo epistulari nec Iulius Titianus sub nominibus illustrium feminarum digna similitudine expressit.
propter quod illum ceteri quique Frontonianorum utpote consectaneum aemulati, cur veternosum dicendi genus imitaretur, oratorum simiam nuncupaverunt.
quibus omnibus ego immane dictu est quantum semper iudicio meo cesserim quantumque servandam singulis pronuntiaverim temporum suorum meritorumque praerogativam.
sed scilicet tibi parui tuaeque examinationi has <litterulas> non recensendas (hoc enim parum est) sed defaecandas, ut aiunt, limandasque commisi, sciens te immodicum esse fautorem non studiorum modo verum etiam studiosorum.
quam ob rem nos nunc perquam haesitabundos in hoc deinceps famae pelagus impellis.
porro autem super huiusmodi opusculo tutius conticueramus, contenti versuum felicius quam peritius editorum opinione, de qua mihi iampridem in portu iudicii publici post lividorum latratuum Scyllas enavigatas sufficientis gloriae ancora sedet.
sed si et hisce deliramentis genuinum molarem invidia non fixerit, actutum tibi a nobis volumina numerosiora percopiosis scaturrientia sermocinationibus multiplicabuntur.
```

Now every line corresponds to a single sentence.

So, in the end we have exported a set of plain text files prepared with some metadata and preprocessing so that they can be easily ingested by the target analysis system without further processing. This will ["macronize"](https://github.com/Myrmex/alatius-macronizer-api) the text, and then proceed further with its prosodical and rhythmic analysis.
