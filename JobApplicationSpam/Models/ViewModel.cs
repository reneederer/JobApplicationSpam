using System.Collections.Generic;
using System;

namespace JobApplicationSpam.Models
{
    public class JobApplicationSpamState
    {
        public Document Document { get; set; }
        public DocumentEmail DocumentEmail { get; set; }
        public IEnumerable<CustomVariable> CustomVariables { get; set; } = new List<CustomVariable>();
        public IEnumerable<DocumentFile> DocumentFiles { get; set; } = new List<DocumentFile>();
        public UserValues UserValues { get; set; }
        public Employer Employer { get; set; }
        public AppUser User;
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
        public string OdtVariable { get; set; } = "";
        public string LabelText { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class TextareaField : VariableField
    {
        public string Height {get;set;} = "";
        public override string GetFieldType() => "_TextAreaField";
    }

    public class InputField : VariableField
    {       
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

    public class ButtonField : VariableField
    {
        public string Class = "";
        public override string GetFieldType() => "_ButtonField";
    }

    public class FileField : Field
    {
        public string Name { get; set; } = "";
        public int Size {get;set;} = 0;
        public string Path {get;set;} = "";
        public override string GetFieldType() => "_FileField";
    }

    public class FileUploadField : Field
    {
        public string Class { get; set; }
        public override string GetFieldType() => "_FileUploadField";
    }

    public class SelectField : VariableField
    {
        public IEnumerable<string> Items { get; set; } = new List<string>();
        public override string GetFieldType() => "_SelectField";
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
