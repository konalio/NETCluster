﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.34014
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 


/// <uwagi/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
public partial class Register
{

    private string typeField;

    private string parallelThreadsField;

    private string deregisterField;

    private string idField;

    private RegisterSolvableProblemsProblemName[] solvableProblemsField;

    /// <uwagi/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <uwagi/>
    public string ParallelThreads
    {
        get
        {
            return this.parallelThreadsField;
        }
        set
        {
            this.parallelThreadsField = value;
        }
    }

    /// <uwagi/>
    public string Deregister
    {
        get
        {
            return this.deregisterField;
        }
        set
        {
            this.deregisterField = value;
        }
    }

    /// <uwagi/>
    public string Id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <uwagi/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ProblemName", typeof(RegisterSolvableProblemsProblemName))]
    public RegisterSolvableProblemsProblemName[] SolvableProblems
    {
        get
        {
            return this.solvableProblemsField;
        }
        set
        {
            this.solvableProblemsField = value;
        }
    }
}

/// <uwagi/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
public partial class RegisterSolvableProblemsProblemName
{

    private string valueField;

    /// <uwagi/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}