//
// MIT License
// 
// Copyright (c) 2020 Brandon Dahn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Fright.Editor.Templates
{
	/// Describes a constructor that can be used in an XML Template
	public class XmlConstructor : XmlFunction
	{
		public string otherConstructArguments = null;
		public string otherConstructType = null;

		public override string xmlType
		{
			get { return "constructor"; }
		}

		public override void ConstructFromXml(XmlNode node, XmlDocument document)
		{
			base.ConstructFromXml(node, document);
			returnType = null;
			GetNameFromParentType(node.ParentNode);

			if (node.GetAttribute("this") is string thisArguments)
			{
				otherConstructArguments = thisArguments;
				otherConstructType = "this";
			}
			else if (node.GetAttribute("base") is string baseArguments)
			{
				otherConstructArguments = baseArguments;
				otherConstructType = "base";
			}
		}

		private void GetNameFromParentType(XmlNode parentNode)
		{
			while(parentNode != null && string.IsNullOrEmpty(id))
			{
				if (parentNode.Name.Equals("class", System.StringComparison.InvariantCultureIgnoreCase) ||
					parentNode.Name.Equals("struct", System.StringComparison.InvariantCultureIgnoreCase))
				{
					id = parentNode.GetAttribute<string>("id");
				}

				parentNode = parentNode.ParentNode;
			}
		}

		/// Converts the XML object into C# and adds it to the string builder
		public override void ToCSharp(StringBuilder stringBuilder, int indentationLevel, TemplateSettings settings)
		{
			WriteSignature(stringBuilder, ref indentationLevel, settings);
			WriteArguments(stringBuilder, ref indentationLevel, settings);

			if (!string.IsNullOrEmpty(otherConstructType) && !string.IsNullOrEmpty(otherConstructArguments))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendIndentations(indentationLevel + 1);
				stringBuilder.AppendFormat(": {0}({1})", otherConstructType, otherConstructArguments);
			}

			WriteBody(stringBuilder, ref indentationLevel, settings);
		}
	}
}