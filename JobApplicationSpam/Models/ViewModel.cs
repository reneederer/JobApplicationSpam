﻿using System.Collections.Generic;
using System;

namespace JobApplicationSpam.Models
{
    public class JobApplicationSpamState
    {
        public Document Document { get; set; }
        public DocumentEmail DocumentEmail { get; set; }
        public IEnumerable<CustomVariable> CustomVariables { get; set; } = new List<CustomVariable>();
        public IEnumerable<DocumentFile> DocumentFiles { get; set; } = new List<DocumentFile>();
        public IEnumerable<SentApplication> SentApplications { get; set; } = new List<SentApplication>();
        public UserValues UserValues { get; set; }
        public AppUser User { get; set; }
    }

    public abstract class Field
    {
        public string Id { get; set; }
        public abstract string GetFieldType();
        public Field()
        {
             Id = Guid.NewGuid().ToString();
        }
    }

    public abstract class VariableField : Field
    {
        public string VariableName { get; set; } = "";
        public string LabelText { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class TextareaField : VariableField
    {
        public int MaxLength { get; set; } = 5000;
        public string Height { get; set; } = "400px";
        public override string GetFieldType() => "_TextAreaField";
    }

    public class InputField : VariableField
    {
        public int MaxLength { get; set; } = 80;
        public override string GetFieldType() => "_InputField";
    }

    public class RadioItem
    {
        public string LabelText { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class RadioField : VariableField
    {
        public IEnumerable<RadioItem> Items { get; set; } = new List<RadioItem>();
        public string GroupName { get; set; } = Guid.NewGuid().ToString();
        public override string GetFieldType() => "_RadioField";
        public RadioField()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class SelectField : Field
    {
        public IEnumerable<string> OptionTexts { get; set; } = new List<string>();
        public override string GetFieldType() => "_SelectField";
    }

    public class ButtonField : Field
    {
        public string Class { get; set; }
        public override string GetFieldType() => "_ButtonField";
        public string LabelText { get; set; }
    }

    public class FileField : Field
    {
        public string Name { get; set; } = "";
        public int Size {get;set;} = 0;
        public string Path {get;set;} = "";
        public override string GetFieldType() => "_FileField";
    }

    public class MenuItem
    {       
        public string Text {get;set;} = "";
        public string FontAwesome {get;set;} = "";
        public string ActivatesPages {get;set;} = "";
    }

    public class Page
    {       
        public string Id {get;set;} = "";
        public string Header {get;set;} = "";
        public IEnumerable<Field> Fields { get; set; } = new List<Field>();
        public bool Active {get;set;} = false;
    }

    public class DocumentFilesField : Field
    {
        public IEnumerable<FileField> Files { get; set; } = new List<FileField>();
        public override string GetFieldType() => "_DocumentFilesField";
    }
}
