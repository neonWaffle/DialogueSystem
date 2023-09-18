using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

public class DialogueParserTest
{
    DialogueParser dialogueParser;
    DialogueVariableRegistry variableRegistry;
    DialogueCommandRegistry commandRegistry;

    [SetUp]
    public void Setup()
    {
        variableRegistry = new DialogueVariableRegistry();
        commandRegistry = new DialogueCommandRegistry();
        dialogueParser = new DialogueParser(variableRegistry, commandRegistry);
    }

    [Test]
    public void DialogueLineTest()
    {
        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            ":"
        }));
    }

    [Test]
    public void IndentationTest()
    {
        variableRegistry.SetVariable("Test", true);

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "\tNPC: Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "\t\tNPC: Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "\tNPC: Line 1",
            "NPC: Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Option 1 line",
            "> Option 2",
            "\t\tNPC: Option 2 line"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "(if $Test == true)",
            "NPC: This should be a part of the branch",
            "NPC: This should escape the branch"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "(if $Test == true)",
            "NPC: This should be a part of the branch",
            "\tNPC: This should be a part of the branch"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "(if $Test == true)",
            "\tNPC: This should be a part of the branch",
            "(else)",
            "NPC: This should be a part of the branch"
        }));

        variableRegistry.SetVariable("HasMetPlayer", false);
        variableRegistry.SetVariable("LikesShiba", true);
        variableRegistry.SetVariable("LikesHuskies", true);

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- BRANCH TEST", //1
            "Alex: Hey!", //2
            "(if $HasMetPlayer == false)", //3
            "\tAlex: I'm Alex. Nice to meet you! ", //4
            "\tAlex: Are you a dog person or a cat person?", //5
            "\t> I prefer dogs", //6
            "\t\tAlex: Same here!", //7
            "\t\t(if $LikesShiba == true and $LikesHuskies == true)", //8
            "\t\t\tAlex: I love shibas and huskies!", //9
            "\t\t\t> I like shibas more.", //10
            "\t\t\t\tAlex: I see your point.", //11
            "\t\t\t> Huskies are my favourite.", //12
            "\t\t\t\tAlex: Yeah, they're great.", //13
            "\t\t\t> I don't really have a favourite.", //14
            "\t\t\t\tAlex: Oh.", //15
            "\t\t(else if $LikesShiba == true)", //16
            "\t\t\tAlex: I love shibas", //17
            "\t\t(else)", //18
            "\t\t\tAlex: I love huskies", //19
            "\t> I prefer cats.", //20
            "\t\tAlex: Oh, I prefer dogs!", //21
            "(else)", //22
            "\tAlex: Nice to see you again!", //23
            "Alex: Bye!" //24
        }));
    }

    [Test]
    public void LoadingTest()
    {
        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Hello there"
        });
        Assert.AreEqual(1, dialogues.Count);

        dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Hello there",
            "- DIALOGUE 2",
            "NPC: Hello there"
        });
        Assert.AreEqual(2, dialogues.Count);

        dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Hello there",
            "",
            "- DIALOGUE 2",
            "NPC: Hello there",
            "- DIALOGUE 3",
            "NPC: Hello there"
        });
        Assert.AreEqual(3, dialogues.Count);

        dialogues = dialogueParser.ParseLines(new string[] {
        });
        Assert.AreEqual(0, dialogues.Count);

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Hello there",
            "",
            "- DIALOGUE 2",
            "NPC: Hello there",
            "",
            "- DIALOGUE 2",
            "NPC: Hello there"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "NPC: Hello there"
        }));
    }

    [Test]
    public void OptionTest()
    {
        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Option 1 line",
            "> Option 2",
            "\tNPC: Option 2 line"
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual(2, dialogueRoot.Options.Count);
        Assert.AreEqual("Option 1 line", dialogueRoot.Options[0].NextLine.Text);
        Assert.AreEqual("Option 2 line", dialogueRoot.Options[1].GetNextAvailableLine().Text);

        dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "> Option 2",
            "> Option 3",
            "NPC: Line 2"
        });

        dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual(3, dialogueRoot.Options.Count);
        Assert.AreEqual("Line 2", dialogueRoot.Options[0].GetNextAvailableLine().Text);
        Assert.AreEqual("Line 2", dialogueRoot.Options[1].GetNextAvailableLine().Text);
        Assert.AreEqual("Line 2", dialogueRoot.Options[2].GetNextAvailableLine().Text);
        Assert.AreEqual("Line 1", dialogueRoot.Options[0].RootLine.Text);
        Assert.AreEqual("Line 1", dialogueRoot.Options[1].RootLine.Text);
        Assert.AreEqual("Line 1", dialogueRoot.Options[2].RootLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[1].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[2].NextLine.Text);

        dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Option 1 line 1",
            "\tNPC: Option 1 line 2",
            "> Option 2",
            "> Option 3",
            "NPC: Line 2"
        });

        dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual(3, dialogueRoot.Options.Count);
        Assert.AreEqual("Option 1 line 2", dialogueRoot.Options[0].GetNextAvailableLine().GetNextAvailableLine().Text);
        Assert.AreEqual("Line 2", dialogueRoot.Options[1].GetNextAvailableLine().Text);
    }

    [Test]
    public void BranchTest()
    {
        variableRegistry.SetVariable("Test", true);

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(else)",
            "\tNPC: Line 2",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(else)",
            "NPC: Line 2",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "(else)",
            "NPC: Line 2",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "NPC: Line 2",
            "(else)",
            "NPC: Line 3",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "NPC: Line 2",
            "(else)",
            "(else)",
            "NPC: Line 3",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "(if $Test == true)",
            "NPC: Line 2",
            "(else)",
            "NPC: Line 3",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\t\tNPC: Line 2",
            "(else)",
            "\tNPC: Line 3",
        }));

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "(else)",
            "\tNPC: Line 3",
            "NPC: Line 4",
        }));

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "\t(if $Test == true)",
            "\t\tNPC: Line 3",
            "(else)",
            "\tNPC: Line 4",
            "NPC: Line 5",
        }));

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "\t(if $Test == true)",
            "\t\tNPC: Line 3",
            "\t\t(if $Test == true)",
            "\t\t\tNPC: Line4",
            "(else)",
            "\tNPC: Line 5",
            "NPC: Line 6",
        }));

        var dialogues = dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "\t(if $Test == true)",
            "\t\tNPC: Line 3",
            "\t\t(if $Test == true)",
            "\t\t\tNPC: Line 4",
            "(else)",
            "\tNPC: Line 5",
            "NPC: Line 6"
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual("Line 2", dialogueRoot.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("Line 3", dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("Line 4", dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("Line 5", dialogueRoot.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Line 6", dialogueRoot.Branch.Branches[1].NextLine.NextLine.Text);

        dialogues = dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "\t(if $Test == true)",
            "\t\tNPC: Line 3",
            "\t\t(if $Test == true)",
            "\t\t\tNPC: Line 4",
            "NPC: Line 5",
            "",
            "- DIALOGUE 2",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Line 2",
            "(else)",
            "\tNPC: Line 3",
            "NPC: Line 4"

        });

        dialogueRoot = dialogues["DIALOGUE 1"];

        Assert.AreEqual("Line 5", dialogueRoot.Branch.Branches[1].NextLine.NextLine.Text);
        Assert.AreEqual("Line 4", dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("Line 5", dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.NextLine.Text);
        Assert.AreEqual("Line 5", dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.NextLine.Text);

        Assert.AreEqual(null, dialogueRoot.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.Text);

        dialogueRoot = dialogues["DIALOGUE 2"];
        Assert.AreEqual("Line 2", dialogueRoot.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("Line 3", dialogueRoot.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Line 4", dialogueRoot.Branch.Branches[0].NextLine.NextLine.Text);
        Assert.AreEqual("Line 4", dialogueRoot.Branch.Branches[1].NextLine.NextLine.Text);

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "(if $Test == true)",
            "\tNPC: Line 2"
        }));
    }

    [Test]
    public void ExpressionTest()
    {
        var dialogues = dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1",
            "NPC [[happy]]: Line 1",
            "NPC [[neutral]]: Line 2",
            "NPC [[sad]]: Line 3",
            "NPC: Line 4"
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual("happy", dialogueRoot.Expression);
        Assert.AreEqual("neutral", dialogueRoot.NextLine.Expression);
        Assert.AreEqual("sad", dialogueRoot.NextLine.NextLine.Expression);
        Assert.AreEqual("neutral", dialogueRoot.NextLine.NextLine.NextLine.Expression);
        Assert.AreEqual("NPC", dialogueRoot.Speaker);
        Assert.AreEqual("Line 1", dialogueRoot.Text);
    }

    [Test]
    public void BranchAndOptionTest()
    {
        variableRegistry.SetVariable("Test", false);

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1", //1
            "NPC: Line 1", //2
            "(if $Test == true)", //3
            "\tNPC: Line 2", //4
            "\t(if $Test == true)", //5
            "\t\tNPC: Line 3", //6
            "\t\t(if $Test == true)", //7
            "\t\t\tNPC: Line 4", //8
            "\t\t\t> Option 1", //9
            "\t\t\t\tNPC: Line 5", //10
            "\t\t\t> Option 2", //11
            "\t\t\t\tNPC: Line 6", //12
            "NPC: Line 7", //13
        }));

        var dialogues = dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1", //1
            "NPC: Line 1", //2
            "(if $Test == true)", //3
            "\tNPC: Line 2", //4
            "\t(if $Test == true)", //5
            "\t\tNPC: Line 3", //6
            "\t\t(if $Test == true)", //7
            "\t\t\tNPC: Line 4", //8
            "\t\t\t> Option 1", //9
            "\t\t\t\tNPC: Line 5", //10
            "\t\t\t> Option 2", //11
            "\t\t\t\tNPC: Line 6", //12
            "NPC: Line 7", //13
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];

        Assert.AreEqual(null, dialogueRoot.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Line 7", dialogueRoot.Branch.Branches[1].NextLine.NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.Text);

        dialogues = dialogueParser.ParseLines(new string[]
        {
            "- DIALOGUE 1", //1
            "NPC: Line 1", //2
            "> Option 1", //3
            "\t(if $Test == true)", //4
            "\t\tNPC: Line 2", //5
            "\t\t(if $Test == true)", //6
            "\t\t\tNPC: Line 3", //7
            "\t\t\t(if $Test == true)", //8
            "\t\t\t\tNPC: Line 4", //9
            "\t\t\t\t> Option 1", //10
            "\t\t\t\t\tNPC: Line 5", //11
            "\t\t\t\t> Option 2", //12
            "\t\t\t\t\tNPC: Line 6", //13
            "> Option 2", //14
            "\tNPC: Option 2 line 1", //15
            "NPC: Line 7", //16
        });

        dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual("Line 2", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Line 3", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Line 4", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Option 1", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[0].Text);
        Assert.AreEqual("Option 2", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[1].Text);
        Assert.AreEqual("Line 5", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("Line 6", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[1].NextLine.Text);
        Assert.AreEqual("Line 7", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[0].NextLine.NextLine.Text);
        Assert.AreEqual("Line 7", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Branch.Branches[0].NextLine.Options[1].NextLine.NextLine.Text);

        variableRegistry.SetVariable("HasMetPlayer", false);
        variableRegistry.SetVariable("LikesShiba", true);
        variableRegistry.SetVariable("LikesHuskies", true);

        dialogues = dialogueParser.ParseLines(new string[]
        {
            "- BRANCH TEST", //1
            "Alex: Hey!", //2
            "(if $HasMetPlayer == false)", //3
            "\tAlex: I'm Alex. Nice to meet you! ", //4
            "\tAlex: Are you a dog person or a cat person?", //5
            "\t> I prefer dogs", //6
            "\t\tAlex: Same here!", //7
            "\t\t(if $LikesShiba == true and $LikesHuskies == true)", //8
            "\t\t\tAlex: I love shibas and huskies!", //9
            "\t\t\t> I like shibas more.", //10
            "\t\t\t\tAlex: I see your point.", //11
            "\t\t\t> Huskies are my favourite.", //12
            "\t\t\t\tAlex: Yeah, they're great.", //13
            "\t\t\t> I don't really have a favourite.", //14
            "\t\t\t\tAlex: Oh.", //15
            "\t\t(else if $LikesShiba == true)", //16
            "\t\t\tAlex: I love shibas", //17
            "\t\t(else)", //18
            "\t\t\tAlex: I love huskies", //19
            "\t> I prefer cats.", //20
            "\t\tAlex: Oh, I prefer dogs!", //21
            "(else)", //22
            "\tAlex: Nice to see you again!", //23
            "Alex: Bye!" //24
        });

        dialogueRoot = dialogues["BRANCH TEST"];

        Assert.AreEqual(2, dialogueRoot.Branch.Branches.Count);
        Assert.AreEqual(2, dialogueRoot.Branch.Branches[0].NextLine.NextLine.Options.Count);
        Assert.AreEqual(3, dialogueRoot.Branch.Branches[0].NextLine.NextLine.Options[0].NextLine.Branch.Branches[0].NextLine.Options.Count);

        variableRegistry.SetVariable("HasMetPlayer", true);
        Assert.AreEqual("Nice to see you again!", dialogueRoot.Branch.Branches[1].NextLine.Text);

        variableRegistry.SetVariable("HasMetPlayer", false);
        variableRegistry.SetVariable("LikesShiba", true);
        variableRegistry.SetVariable("LikesHuskies", true);
        variableRegistry.SetVariable("LikesCats", true);

        dialogues = dialogueParser.ParseLines(new string[]
        {
            "- BRANCH TEST", //1
            "Alex: Hey!", //2
            "(if $HasMetPlayer == false)", //3
            "\tAlex: I'm Alex. Nice to meet you! ", //4
            "\tAlex: Are you a dog person or a cat person?", //5
            "\t> I prefer dogs", //6
            "\t\tAlex: Same here!", //7
            "\t\t(if $LikesShiba == true and $LikesHuskies == true)", //8
            "\t\t\tAlex: I love shibas and huskies!", //9
            "\t\t\t> I like shibas more.", //10
            "\t\t\t\tAlex: I see your point.", //11
            "\t\t\t> Huskies are my favourite.", //12
            "\t\t\t\tAlex: Yeah, they're great.", //13
            "\t\t\t> I don't really have a favourite.", //14
            "\t\t\t\tAlex: Oh.", //15
            "\t\t\t\t(if $LikesCats == true)", //15
            "\t\t\t\t\tAlex: How do you feel about cats though?", //16
            "\t\t(else if $LikesShiba == true)", //17
            "\t\t\tAlex: I love shibas", //18
            "\t\t(else)", //19
            "\t\t\tAlex: I love huskies", //20
            "\t> I prefer cats.", //21
            "\t\tAlex: Oh, I prefer dogs!", //22
            "(else)", //23
            "\tAlex: Nice to see you again!", //24
            "Alex: Bye!" //25
        });

        dialogueRoot = dialogues["BRANCH TEST"];

        Assert.AreEqual("How do you feel about cats though?",
            dialogueRoot.Branch.Branches[0].NextLine.NextLine.Options[0].NextLine.Branch.Branches[0].NextLine.Options[2].NextLine.Branch.Branches[0].NextLine.Text);
    }

    [Test]
    public void CommandTest()
    {
        variableRegistry.SetVariable("Test", true);
        variableRegistry.SetVariable("AnotherTest", false);
        commandRegistry.AddCommand("Print", new Action<string>((input) => Console.WriteLine(input)));

        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1 (Print(\"Test\"); Print(\"Another test\"); Print(\"Another test\"))",
            "NPC: Line 2",
            "NPC: Line 3 (Print(\"Test\"))",
            "",
            "- DIALOGUE 2",
            "NPC: Line 1 ($Test = false)",
            "NPC: Line 2 ($AnotherTest = false; $Test = true)",
            "NPC: Line 2 ($AnotherTest = false; $Test = true; Print(\"Test\"))",
            "NPC: Line 2 ($AnotherTest = false; $Test = true; Print(\"Test\"); Print(\"Test\"))",
            "",
            "- DIALOGUE 3",
            "NPC: Line 1",
            "> Option 1 ($Test = false)",
            "\tNPC: Line 2",
            "> Option 2 (Print(\"Test\"))",
            "\tNPC: Line 3",
            "> Option 3",
            "\tNPC: Line 4"
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual(3, dialogueRoot.Commands.Count);
        Assert.AreEqual(0, dialogueRoot.NextLine.Commands.Count);
        Assert.AreEqual(1, dialogueRoot.NextLine.NextLine.Commands.Count);
        Assert.AreEqual("Print", dialogueRoot.Commands[0].CommandName);
        Assert.AreEqual("Print", dialogueRoot.Commands[1].CommandName);
        Assert.AreEqual("Print", dialogueRoot.Commands[2].CommandName);
        Assert.AreEqual(1, dialogueRoot.Commands[0].Args.Length);
        Assert.AreEqual(1, dialogueRoot.Commands[1].Args.Length);
        Assert.AreEqual(1, dialogueRoot.Commands[2].Args.Length);
        Assert.AreEqual(0, dialogueRoot.NextLine.Commands.Count);
        Assert.AreEqual(1, dialogueRoot.NextLine.NextLine.Commands.Count);
        Assert.AreEqual("Line 1", dialogueRoot.Text);
        Assert.AreEqual("Line 2", dialogueRoot.NextLine.Text);
        Assert.AreEqual("Line 3", dialogueRoot.NextLine.NextLine.Text);

        dialogueRoot = dialogues["DIALOGUE 2"];
        Assert.AreEqual(1, dialogueRoot.Commands.Count);
        Assert.AreEqual(2, dialogueRoot.NextLine.Commands.Count);
        Assert.AreEqual(3, dialogueRoot.NextLine.NextLine.Commands.Count);
        Assert.AreEqual(4, dialogueRoot.NextLine.NextLine.NextLine.Commands.Count);

        dialogueRoot = dialogues["DIALOGUE 3"];
        Assert.AreEqual(1, dialogueRoot.Options[0].Commands.Count);
        Assert.AreEqual(1, dialogueRoot.Options[1].Commands.Count);
        Assert.AreEqual(0, dialogueRoot.Options[2].Commands.Count);

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1 (Print(\"Test\", \"Test\"))"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1 (Print())"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1 (Test())"
        }));
    }

    [Test]
    public void ConditionTest()
    {
        variableRegistry.SetVariable("Test", true);
        variableRegistry.SetVariable("AnotherTest", false);

        commandRegistry.AddCommand("TestCondition", new Func<bool>(() => true));

        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "(if $Test == true)",
            "\tNPC: Branch 1",
            "(else if $Test == false and $AnotherTest == true)",
            "\tNPC: Branch 2",
            "(else if $Test == false and TestCondition())",
            "\tNPC: Branch 3",
            "(else)",
            "\tNPC: Branch 4"
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];

        Assert.AreEqual(1, dialogueRoot.Branch.Branches[0].Conditions.Count);
        Assert.AreEqual(2, dialogueRoot.Branch.Branches[1].Conditions.Count);
        Assert.AreEqual(2, dialogueRoot.Branch.Branches[2].Conditions.Count);
        Assert.AreEqual(0, dialogueRoot.Branch.Branches[3].Conditions.Count);

        commandRegistry.AddCommand("TestConditionWrong", new Action(() => { }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "(if TestConditionWrong())",
            "\tNPC: Line 1",
        }));
    }

    [Test]
    public void MiscTest()
    {
        variableRegistry.SetVariable("Test", true);

        Assert.DoesNotThrow(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "NPC: Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "NPC: Line 1",
            "NPC: Line 2"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "(if $Test == true)",
            "\tNPC: Line 1",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "> Option 1",
            "\tNPC: Line 1",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "",
            "- DIALOGUE 1",
            "NPC: Line 1"
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1"
        }));
    }

    [Test]
    public void DialogueLineReturnTest()
    {
        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Line 2 <-",
            "> Option 2",
            "\tNPC: Line 3",
            "NPC: Line 4",
            "",
            "- DIALOGUE 2",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Option 1 line 1",
            "\t> Nested option 1",
            "\t\tNPC: Nested option 1 line 1",
            "\t> Nested option 2",
            "\t\tNPC: Nested option 2 line 1 <-",
            "\t> Nested option 3",
            "\t\tNPC: Nested option 3 line 1 <- <-",
            "\t> Nested option 4",
            "\t\tNPC: Nested option 4 line 1",
            "\t\t> Nested nested option 1",
            "\t\t\tNPC: Nested nested option 1 line 1 <- <- <-",
            "> Option 2",
            "\tNPC: Option 2 line 1",
            "NPC: Line 2",
        });

        var dialogueRoot = dialogues["DIALOGUE 1"];
        Assert.AreEqual("Line 1", dialogueRoot.Options[0].NextLine.NextLine.Text);

        dialogueRoot = dialogues["DIALOGUE 2"];
        Assert.AreEqual("Line 2", dialogueRoot.Options[0].NextLine.NextLine.Text);
        Assert.AreEqual("Option 1 line 1", dialogueRoot.Options[0].NextLine.Options[1].NextLine.NextLine.Text);
        Assert.AreEqual("Nested option 3 line 1", dialogueRoot.Options[0].NextLine.Options[2].NextLine.Text);
        Assert.AreEqual("Line 1", dialogueRoot.Options[0].NextLine.Options[2].NextLine.NextLine.Text);
        Assert.AreEqual("Nested nested option 1 line 1", dialogueRoot.Options[0].NextLine.Options[3].NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("Line 1", dialogueRoot.Options[0].NextLine.Options[3].NextLine.Options[0].NextLine.NextLine.Text);

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Line 2 <- <-",
            "> Option 2",
            "\tNPC: Line 3",
            "NPC: Line 4",
        }));

        Assert.Throws<Exception>(() => dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Line 2 <- <-",
            "> Option 2",
            "\tNPC: Line 3",
            "NPC: Line 4",
            "",
            "- DIALOGUE 2",
            "NPC: Line 1",
            "> Option 1",
            "\tNPC: Option 1 line 1",
            "\t> Nested option 1",
            "\t\tNPC: Nested option 1 line 1",
            "\t> Nested option 2",
            "\t\tNPC: Nested option 2 line 1 <-",
            "\t> Nested option 3",
            "\t\tNPC: Nested option 3 line 1 <- <-",
            "\t> Nested option 4",
            "\t\tNPC: Nested option 4 line 1",
            "\t\t> Nested nested option 1",
            "\t\t\tNPC: Nested nested option 1 line 1 <- <- <- <-",
            "> Option 2",
            "\tNPC: Option 2 line 1",
            "NPC: Line 2",
        }));
    }

    [Test]
    public void Comments()
    {
        var dialogues = dialogueParser.ParseLines(new string[] {
            "- DIALOGUE 1",
            "NPC: Hey there! //This is a comment",
            "> Option 1 //This is a comment",
            "\tNPC: Option 1 line 1 //This is a comment",
            "> Option 2 //This is a comment",
            "\tNPC: Option 2 line 1 //This is a comment"
        });

        var dialogueLine = dialogues["DIALOGUE 1"];
        Assert.AreEqual("Hey there!", dialogueLine.Text);
        Assert.AreEqual("Option 1", dialogueLine.Options[0].Text);
        Assert.AreEqual("Option 1 line 1", dialogueLine.Options[0].NextLine.Text);
        Assert.AreEqual("Option 2", dialogueLine.Options[1].Text);
        Assert.AreEqual("Option 2 line 1", dialogueLine.Options[1].NextLine.Text);
    }

    [Test]
    public void TestDialogue()
    {
        var dialogues = dialogueParser.ParseLines(new string[] {
            "- LINEAR DIALOGUE", //1
            "Alice: Hey there!", //2
            "Alice: I hope you're doing well!", //3
            "Alice: You can speak to everyone here to learn more about different features of the dialogue system.", //4
            "Alice: For example, I'm showing you basic linear dialogue right now.", //5
        });

        var dialogueRoot = dialogues["LINEAR DIALOGUE"];
        Assert.AreEqual("Hey there!", dialogueRoot.Text);
        Assert.AreEqual("I hope you're doing well!", dialogueRoot.NextLine.Text);
        Assert.AreEqual("You can speak to everyone here to learn more about different features of the dialogue system.", dialogueRoot.NextLine.NextLine.Text);
        Assert.AreEqual("For example, I'm showing you basic linear dialogue right now.", dialogueRoot.NextLine.NextLine.NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.NextLine.NextLine.NextLine.NextLine);


        variableRegistry.SetVariable("HasMetPlayer", false);
        dialogues = dialogueParser.ParseLines(new string[] {
            "- BRANCHES", //1
            "Alex: Hey!", //2
            "(if $HasMetPlayer == false)", //3
            "\tAlex: I'm Alex. Nice to meet you! ($HasMetPlayer = true)", //4
            "\tAlex: You can talk with me again and you'll get a different response now", //5
            "(else)", //6
            "\tAlex: Nice to see you again!", //7
            "Alex: Bye!", //8
        });

        dialogueRoot = dialogues["BRANCHES"];
        Assert.AreEqual("Hey!", dialogueRoot.Text);
        Assert.AreEqual("I'm Alex. Nice to meet you!", dialogueRoot.Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("You can talk with me again and you'll get a different response now", dialogueRoot.Branch.Branches[0].NextLine.NextLine.Text);
        Assert.AreEqual("Bye!", dialogueRoot.Branch.Branches[0].NextLine.NextLine.NextLine.Text);
        Assert.AreEqual("Nice to see you again!", dialogueRoot.Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("Bye!", dialogueRoot.Branch.Branches[1].NextLine.NextLine.Text);


        dialogues = dialogueParser.ParseLines(new string[] {
            "- EXPRESSIONS", //1
            "Alice: How are you doing?", //2
            "> I'm doing okay.", //3
            "\tAlice [[happy]]: That's great!", //4
            "> Could be better.", //5
            "\tAlice [[sad]]: I'm sorry to hear that.", //6
        });

        dialogueRoot = dialogues["EXPRESSIONS"];
        Assert.AreEqual("How are you doing?", dialogueRoot.Text);
        Assert.AreEqual("neutral", dialogueRoot.Expression);
        Assert.AreEqual("I'm doing okay.", dialogueRoot.Options[0].Text);
        Assert.AreEqual("That's great!", dialogueRoot.Options[0].NextLine.Text);
        Assert.AreEqual("happy", dialogueRoot.Options[0].NextLine.Expression);
        Assert.AreEqual("Could be better.", dialogueRoot.Options[1].Text);
        Assert.AreEqual("I'm sorry to hear that.", dialogueRoot.Options[1].NextLine.Text);
        Assert.AreEqual("sad", dialogueRoot.Options[1].NextLine.Expression);


        dialogues = dialogueParser.ParseLines(new string[] {
            "- RETURNING FROM OPTIONS", //1
            "Alice: You can have dialogue options that return to the dialogue line from which they were started.", //2
            "Alice: You can try it out.", //3
            "> This option will make the dialogue return to the previous line after its response.", //4
            "\tAlice: We'll be going back now. <-", //5
            "> This option will continue the dialogue as usual after its response.", //6
            "\tAlice: The dialogue will now go further.", //7
            "Alice: This also works with nested options!", //8
            "> This option will lead you to more nested options.", //9
            "\tAlice: You can also choose how many options you want to return.", //10
            "\t> This option will make the dialogue return to the previous line after its response.", //11
            "\t\tAlice: We'll be going back by one line now. <-", //12
            "\t> This option will make the dialogue return to the line before the previous line after its response.", //13
            "\t\tAlice: We'll be going back by two lines now. <- <-", //14
            "\t> This option will continue the dialogue as usual after its response.", //15
            "\t\tAlice: The dialogue will now go further.", //16
            "> This option will continue the dialogue as usual after its response.", //17
            "\tAlice: The dialogue will now go further.", //18
            "Alice: That's it for returning!" //19
        });

        dialogueRoot = dialogues["RETURNING FROM OPTIONS"];
        Assert.AreEqual("You can have dialogue options that return to the dialogue line from which they were started.", dialogueRoot.Text);
        Assert.AreEqual("You can try it out.", dialogueRoot.NextLine.Text);
        Assert.AreEqual("This option will make the dialogue return to the previous line after its response.", dialogueRoot.NextLine.Options[0].Text);
        Assert.AreEqual("We'll be going back now.", dialogueRoot.NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("This option will continue the dialogue as usual after its response.", dialogueRoot.NextLine.Options[1].Text);
        Assert.AreEqual("The dialogue will now go further.", dialogueRoot.NextLine.Options[1].NextLine.Text);
        Assert.AreEqual("You can try it out.", dialogueRoot.NextLine.Options[0].NextLine.NextLine.Text);
        Assert.AreEqual("This also works with nested options!", dialogueRoot.NextLine.Options[1].NextLine.NextLine.Text);
        Assert.AreEqual("This option will lead you to more nested options.", dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].Text);
        Assert.AreEqual("You can also choose how many options you want to return.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("This option will make the dialogue return to the previous line after its response.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[0].Text);
        Assert.AreEqual("We'll be going back by one line now.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("This option will make the dialogue return to the line before the previous line after its response.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[1].Text);
        Assert.AreEqual("We'll be going back by two lines now.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[1].NextLine.Text);
        Assert.AreEqual("This also works with nested options!",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[1].NextLine.NextLine.Text);
        Assert.AreEqual("This option will continue the dialogue as usual after its response.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[2].Text);
        Assert.AreEqual("The dialogue will now go further.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[2].NextLine.Text);
        Assert.AreEqual("That's it for returning!",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[0].NextLine.Options[2].NextLine.NextLine.Text);
        Assert.AreEqual("This option will continue the dialogue as usual after its response.",
            dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[1].Text);
        Assert.AreEqual("The dialogue will now go further.", dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[1].NextLine.Text);
        Assert.AreEqual("That's it for returning!", dialogueRoot.NextLine.Options[1].NextLine.NextLine.Options[1].NextLine.NextLine.Text);


        dialogues = dialogueParser.ParseLines(new string[] {
            "- OPTIONS", //1
            "Alex: What's your favourite food?", //2
            "> Pizza!", //3
            "\tAlex: Do you think pineapples belong on pizza?", //4
            "\t> Of course, they do!", //5
            "\t\tAlex: I knew we'd get along!", //6
            "\t> No.", //7
            "\t\tAlex: Well, I can't say your choice surprises me.", //8
            "> Salad!", //9
            "\tAlex: What's your favourite salad?", //10
            "\t> Greek salad.", //11
            "\t\tAlex: Same here!", //12
            "\t> Caesar salad.", //13
            "\t\tAlex: Nice choice!", //14
        });

        dialogueRoot = dialogues["OPTIONS"];
        Assert.AreEqual("What's your favourite food?", dialogueRoot.Text);
        Assert.AreEqual("Pizza!", dialogueRoot.Options[0].Text);
        Assert.AreEqual("Do you think pineapples belong on pizza?", dialogueRoot.Options[0].NextLine.Text);
        Assert.AreEqual("Of course, they do!", dialogueRoot.Options[0].NextLine.Options[0].Text);
        Assert.AreEqual("I knew we'd get along!", dialogueRoot.Options[0].NextLine.Options[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].NextLine.Options[0].NextLine.NextLine);
        Assert.AreEqual("No.", dialogueRoot.Options[0].NextLine.Options[1].Text);
        Assert.AreEqual("Well, I can't say your choice surprises me.", dialogueRoot.Options[0].NextLine.Options[1].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].NextLine.Options[1].NextLine.NextLine);
        Assert.AreEqual("Salad!", dialogueRoot.Options[1].Text);
        Assert.AreEqual("What's your favourite salad?", dialogueRoot.Options[1].NextLine.Text);
        Assert.AreEqual("Greek salad.", dialogueRoot.Options[1].NextLine.Options[0].Text);
        Assert.AreEqual("Same here!", dialogueRoot.Options[1].NextLine.Options[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[1].NextLine.Options[0].NextLine.NextLine);
        Assert.AreEqual("Caesar salad.", dialogueRoot.Options[1].NextLine.Options[1].Text);
        Assert.AreEqual("Nice choice!", dialogueRoot.Options[1].NextLine.Options[1].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[1].NextLine.Options[1].NextLine.NextLine);


        commandRegistry.AddCommand("SkillCheck", new Func<int, bool>((num) => true));
        commandRegistry.AddCommand("IncreaseSkill", new Action<int>((num) => { }));
        dialogues = dialogueParser.ParseLines(new string[] {
            "- CONDITIONAL OPTIONS", //1
            "Alex: Some dialogue options have conditions.", //2
            "> (if SkillCheck(10)) Do skill check.", //3
            "\tAlex: You've passed skill check!", //4
            "> This option will increase your skill allowing you to select the skill check option. (IncreaseSkill(5))", //5
            "\tAlex: You will be able to choose the skill check now! <-", //6
        });

        dialogueRoot = dialogues["CONDITIONAL OPTIONS"];
        Assert.AreEqual("Some dialogue options have conditions.", dialogueRoot.Text);
        Assert.AreEqual("Do skill check.", dialogueRoot.Options[0].Text);
        Assert.AreEqual(1, dialogueRoot.Options[0].Conditions.Count);
        Assert.AreEqual("You've passed skill check!", dialogueRoot.Options[0].NextLine.Text);
        Assert.AreEqual(null, dialogueRoot.Options[0].NextLine.NextLine);
        Assert.AreEqual("This option will increase your skill allowing you to select the skill check option.", dialogueRoot.Options[1].Text);
        Assert.AreEqual(0, dialogueRoot.Options[1].Conditions.Count);
        Assert.AreEqual("You will be able to choose the skill check now!", dialogueRoot.Options[1].NextLine.Text);
        Assert.AreEqual("Some dialogue options have conditions.", dialogueRoot.Options[1].NextLine.NextLine.Text);


        commandRegistry.AddCommand("RollDice", new Func<int, bool>((num) => true));
        dialogues = dialogueParser.ParseLines(new string[]
        {
            "- OPTIONS WITH BRANCHES",
            "Alice: Dialogue options can also have branches.",
            "> This option will roll the dice, which will determine which dialogue branch you'll get afterwards.",
            "\t(if RollDice(10))",
            "\t\tAlice: You've rolled 10 or more.",
            "\t(else)",
            "\t\tAlice: You've rolled less than 10.",
            "\tAlice: This line is shown regardless of the roll. Would you like to try again?",
            "\t> Yes.",
            "\t\tAlice: Okay! <- <-",
            "\t> No.",
            "\t\tAlice: Okay.",
            "> I see.",
            "Alice: That's it for options and branches."
        });

        dialogueRoot = dialogues["OPTIONS WITH BRANCHES"];
        Assert.AreEqual("This option will roll the dice, which will determine which dialogue branch you'll get afterwards.",
            dialogueRoot.Options[0].Text);
        Assert.AreEqual("You've rolled 10 or more.", dialogueRoot.Options[0].Branch.Branches[0].NextLine.Text);
        Assert.AreEqual("You've rolled less than 10.", dialogueRoot.Options[0].Branch.Branches[1].NextLine.Text);
        Assert.AreEqual("This line is shown regardless of the roll. Would you like to try again?",
            dialogueRoot.Options[0].Branch.Branches[0].NextLine.NextLine.Text);
        Assert.AreEqual("This line is shown regardless of the roll. Would you like to try again?",
            dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Text);
        Assert.AreEqual("Yes.", dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[0].Text);
        Assert.AreEqual("Okay!", dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[0].NextLine.Text);
        Assert.AreEqual("Dialogue options can also have branches.",
            dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[0].NextLine.NextLine.Text);
        Assert.AreEqual("No.", dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[1].Text);
        Assert.AreEqual("Okay.", dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[1].NextLine.Text);
        Assert.AreEqual("That's it for options and branches.", dialogueRoot.Options[0].Branch.Branches[1].NextLine.NextLine.Options[1].NextLine.NextLine.Text);
        Assert.AreEqual("I see.", dialogueRoot.Options[1].Text);
        Assert.AreEqual(null, dialogueRoot.Options[1].NextLine.Text);
        Assert.AreEqual("That's it for options and branches.", dialogueRoot.Options[1].NextLine.NextLine.Text);
    }
}
