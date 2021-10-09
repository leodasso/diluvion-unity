using UnityEngine;
using System.Text;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		#region I2CSV format

		public string Export_I2CSV( string Category, char Separator = ',' )
		{
			StringBuilder Builder = new StringBuilder ();

			//--[ Header ]----------------------------------
			Builder.Append ("Key[*]Type[*]Desc");
			foreach (LanguageData langData in mLanguages)
			{
				Builder.Append ("[*]");
				if (!langData.IsEnabled())
					Builder.Append('$');
				Builder.Append ( GoogleLanguages.GetCodedLanguage(langData.Name, langData.Code) );
			}
			Builder.Append ("[ln]");

			int nLanguages = (mLanguages.Count);
			bool firstLine = true;
			foreach (TermData termData in mTerms)
			{
				string Term;
				
				if (string.IsNullOrEmpty(Category) || (Category==EmptyCategory && termData.Term.IndexOfAny(CategorySeparators)<0))
					Term = termData.Term;
				else
					if (termData.Term.StartsWith(Category + @"/") && Category!=termData.Term)
						Term = termData.Term.Substring(Category.Length+1);
				else
					continue;   // Term doesn't belong to this category


				if (!firstLine) Builder.Append("[ln]");
				else firstLine = false;

				AppendI2Term( Builder, nLanguages, Term, termData, string.Empty, termData.Languages, termData.Languages_Touch, Separator, (byte)TranslationFlag.AutoTranslated_Normal, (byte)TranslationFlag.AutoTranslated_Touch);

				if (termData.HasTouchTranslations())
				{
					if (!firstLine) Builder.Append("[ln]");
								else firstLine = false;
					AppendI2Term(Builder, nLanguages, Term, termData, "[touch]", termData.Languages_Touch, null, Separator, (byte)TranslationFlag.AutoTranslated_Touch, (byte)TranslationFlag.AutoTranslated_Normal);
				}
			}
			return Builder.ToString();
		}

		static void AppendI2Term( StringBuilder Builder, int nLanguages, string Term, TermData termData, string postfix, string[] aLanguages, string[] aSecLanguages, char Separator, byte FlagBitMask, byte SecFlagBitMask )
		{
			//--[ Key ] --------------
			Builder.Append (Term);
			Builder.Append( postfix );
			Builder.Append ("[*]");

			//--[ Type and Description ] --------------
			Builder.Append (termData.TermType.ToString());
			Builder.Append ("[*]");
			Builder.Append (termData.Description);

			//--[ Languages ] --------------
			for (int i=0; i<Mathf.Min (nLanguages, aLanguages.Length); ++i)
			{
				Builder.Append ("[*]");
				
				string translation = aLanguages[i];
				//bool isAutoTranslated = ((termData.Flags[i]&FlagBitMask)>0);
				if (string.IsNullOrEmpty(translation) && aSecLanguages!=null)
				{
					translation = aSecLanguages[i];
					//isAutoTranslated = ((termData.Flags[i]&SecFlagBitMask)>0);
				}
				
				//if (string.IsNullOrEmpty(s))
				//	s = "-";
				
				//if (isAutoTranslated) Builder.Append("[i2auto]");
				Builder.Append(translation);
			}
		}

		#endregion

		#region CSV format

		public string Export_CSV( string Category, char Separator = ',' )
		{
			StringBuilder Builder = new StringBuilder();
			
			int nLanguages = (mLanguages.Count);
			Builder.AppendFormat ("Key{0}Type{0}Desc", Separator);

			foreach (LanguageData langData in mLanguages)
			{
				Builder.Append (Separator);
				if (!langData.IsEnabled())
					Builder.Append('$');
				AppendString ( Builder, GoogleLanguages.GetCodedLanguage(langData.Name, langData.Code), Separator );
			}
			Builder.Append ("\n");


			// Sort Terms
			for (int i=0; i<mTerms.Count-1; i++)
				for (int j=i+1; j<mTerms.Count; j++)
					if (string.CompareOrdinal( mTerms[i].Term, mTerms[j].Term)>0 )
					{
						var temp = mTerms[i];
						mTerms[i] = mTerms[j];
						mTerms[j] = temp;
					}
			//mTerms = mTerms.OrderBy( x => x.Term ).ToList();

			foreach (TermData termData in mTerms)
			{
				string Term;

				if (string.IsNullOrEmpty(Category) || (Category==EmptyCategory && termData.Term.IndexOfAny(CategorySeparators)<0))
					Term = termData.Term;
				else
				if (termData.Term.StartsWith(Category + @"/") && Category!=termData.Term)
					Term = termData.Term.Substring(Category.Length+1);
				else
					continue;	// Term doesn't belong to this category

				AppendTerm( Builder, nLanguages, Term, termData, null, termData.Languages, termData.Languages_Touch, Separator, (byte)TranslationFlag.AutoTranslated_Normal, (byte)TranslationFlag.AutoTranslated_Touch);

				if (termData.HasTouchTranslations())
					AppendTerm( Builder, nLanguages, Term, termData, "[touch]", termData.Languages_Touch, null, Separator, (byte)TranslationFlag.AutoTranslated_Touch, (byte)TranslationFlag.AutoTranslated_Normal);
			}
			return Builder.ToString();
		}

		static void AppendTerm( StringBuilder Builder, int nLanguages, string Term, TermData termData, string prefix, string[] aLanguages, string[] aSecLanguages, char Separator, byte FlagBitMask, byte SecFlagBitMask )
		{
			//--[ Key ] --------------				
			AppendString( Builder, Term, Separator );

			if (!string.IsNullOrEmpty(prefix))
				Builder.Append( prefix );
			
			//--[ Type and Description ] --------------
			Builder.Append (Separator);
			Builder.Append (termData.TermType.ToString());
			Builder.Append (Separator);
			AppendString(Builder, termData.Description, Separator);
			
			//--[ Languages ] --------------
			for (int i=0; i<Mathf.Min (nLanguages, aLanguages.Length); ++i)
			{
				Builder.Append (Separator);

				string s = aLanguages[i];
				//bool isAutoTranslated = ((termData.Flags[i]&FlagBitMask)>0);
				if (string.IsNullOrEmpty(s) && aSecLanguages!=null)
				{
					s = aSecLanguages[i];
					//isAutoTranslated = ((termData.Flags[i]&SecFlagBitMask)>0);
				}

				//if (string.IsNullOrEmpty(s))
				//	s = "-";

				AppendTranslation(Builder, s, Separator, /*isAutoTranslated ? "[i2auto]" : */string.Empty);
			}
			Builder.Append ("\n");
		}
		
		
		static void AppendString( StringBuilder Builder, string Text, char Separator )
		{
			if (string.IsNullOrEmpty(Text))
				return;
			Text = Text.Replace ("\\n", "\n");
			if (Text.IndexOfAny((Separator+"\n\"").ToCharArray())>=0)
			{
				Text = Text.Replace("\"", "\"\"");
				Builder.AppendFormat("\"{0}\"", Text);
			}
			else 
			{
				Builder.Append (Text);
			}
		}

		static void AppendTranslation( StringBuilder Builder, string Text, char Separator, string tags )
		{
			if (string.IsNullOrEmpty(Text))
				return;
			Text = Text.Replace ("\\n", "\n");
			if (Text.IndexOfAny((Separator+"\n\"").ToCharArray())>=0)
			{
				Text = Text.Replace("\"", "\"\"");
				Builder.AppendFormat("\"{0}{1}\"", tags, Text);
			}
			else 
			{
				Builder.Append (tags);
				Builder.Append (Text);
			}
		}


		#endregion
	}
}