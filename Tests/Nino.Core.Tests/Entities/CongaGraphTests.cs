// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Entities.Conga;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Tests.Entities;

public class CongaGraphTests
{
    private static readonly Abbreviation A = Abbreviation.From("A");
    private static readonly Abbreviation B = Abbreviation.From("B");
    private static readonly Abbreviation C = Abbreviation.From("C");
    private static readonly Abbreviation D = Abbreviation.From("D");
    private static readonly Abbreviation E = Abbreviation.From("E");
    private static readonly Abbreviation Group1 = Abbreviation.From("@GROUP1");
    private static readonly Abbreviation Group2 = Abbreviation.From("@GROUP2");

    [Test]
    public async Task Add_Edge_With_No_Existing_Nodes_Adds_Both_Nodes_And_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        await Assert.That(g.Children).Contains(n => n.Name == A);
        await Assert.That(g.Children).Contains(n => n.Name == B);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(b.Prerequisites).Contains(a);
    }

    [Test]
    public async Task Add_Edge_With_One_Existing_Node_Adds_Other_Node_and_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, C);

        await Assert.That(g.Children).Contains(n => n.Name == A);
        await Assert.That(g.Children).Contains(n => n.Name == B);
        await Assert.That(g.Children).Contains(n => n.Name == C);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);
        var c = g.Nodes.Single(n => n.Name == C);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(b.Prerequisites).Contains(a);

        await Assert.That(b.Dependents).Contains(c);
        await Assert.That(c.Prerequisites).Contains(b);
    }

    [Test]
    public async Task Add_Edge_With_Two_Existing_Node_Adds_No_Nodes_And_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, C);
        g.AddEdge(A, C);

        await Assert.That(g.Children).Contains(n => n.Name == A);
        await Assert.That(g.Children).Contains(n => n.Name == B);
        await Assert.That(g.Children).Contains(n => n.Name == C);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);
        var c = g.Nodes.Single(n => n.Name == C);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(a.Dependents).Contains(c);

        await Assert.That(b.Prerequisites).Contains(a);
        await Assert.That(b.Dependents).Contains(c);
        await Assert.That(b.Dependents).Contains(c);

        await Assert.That(c.Prerequisites).Contains(b);
        await Assert.That(c.Prerequisites).Contains(a);
    }

    [Test]
    public async Task Add_Edge_With_New_Group_Creates_Group_and_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, Group1);

        await Assert.That(g.Children).Contains(n => n.Name == A);
        await Assert.That(g.Children).Contains(n => n.Name == B);
        await Assert.That(g.Children).Contains(n => n.Name == Group1);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);
        var group = g.Nodes.Single(n => n.Name == Group1);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(b.Prerequisites).Contains(a);

        await Assert.That(b.Dependents).Contains(group);
        await Assert.That(group.Prerequisites).Contains(b);
    }

    [Test]
    public async Task Add_Edge_With_Existing_Group_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddGroup(Group1);
        g.AddEdge(B, Group1);

        await Assert.That(g.Children).Contains(n => n.Name == A);
        await Assert.That(g.Children).Contains(n => n.Name == B);
        await Assert.That(g.Children).Contains(n => n.Name == Group1);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);
        var group = g.Nodes.Single(n => n.Name == Group1);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(b.Prerequisites).Contains(a);

        await Assert.That(b.Dependents).Contains(group);
        await Assert.That(group.Prerequisites).Contains(b);
    }

    [Test]
    public async Task Add_Duplicate_Edge_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, C);
        var result = g.AddEdge(A, B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Duplicate);
    }

    [Test]
    public async Task Add_Self_Edge_Returns_Error()
    {
        var g = new CongaGraph();

        var result = g.AddEdge(A, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.SelfLoop);
    }

    [Test]
    public async Task Add_Cyclic_Edge_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, C);
        var result = g.AddEdge(C, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Cycle);
    }

    [Test]
    public async Task Add_Group_Adds_Group()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);

        await Assert.That(g.Children).Contains(n => n.Name == Group1);
    }

    [Test]
    public async Task Add_Duplicate_Group_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        var result = g.AddGroup(Group1);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Duplicate);
    }

    [Test]
    public async Task Add_Member_To_Group_Adds_Member_To_Group()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);

        await Assert.That(g.Children).Contains(n => n.Name == Group1);
        await Assert.That(g.Nodes).Contains(n => n.Name == A);
        await Assert.That(g.Children).DoesNotContain(n => n.Name == A);

        var group = g.Nodes.Single(n => n.Name == Group1) as CongaNode.GroupNode;

        await Assert.That(group).IsNotNull();
        await Assert.That(group.Children).Contains(n => n.Name == A);
    }

    [Test]
    public async Task Add_Duplicate_Member_To_Group_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        var result = g.AddGroupMember(Group1, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.DuplicateMember);
    }

    [Test]
    public async Task Add_Outside_Node_To_Group_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        var result = g.AddGroupMember(Group1, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Duplicate);
    }

    [Test]
    public async Task Add_To_Group_That_Does_Not_Exist_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        var result = g.AddGroupMember(Group1, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.NoGroup);
    }

    [Test]
    public async Task Add_To_Group_That_Is_Not_A_Group_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        var result = g.AddGroupMember(B, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.NoGroup);
    }

    [Test]
    public async Task Add_Edge_Inside_Group_Adds_Edge_In_Group()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);

        var group = g.Nodes.Single(n => n.Name == Group1) as CongaNode.GroupNode;
        await Assert.That(group).IsNotNull();

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(group.Children).Contains(n => n.Name == A);
        await Assert.That(group.Children).Contains(n => n.Name == B);

        await Assert.That(a.Dependents).Contains(b);
        await Assert.That(b.Prerequisites).Contains(a);
    }

    [Test]
    public async Task Add_Edge_Inside_Group_To_Outside_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(B, C);
        var result = g.AddEdge(A, B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.MixedGroups);
    }

    [Test]
    public async Task Add_Edge_Inside_Group_To_Different_Group_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroup(Group2);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group2, B);
        var result = g.AddEdge(A, B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.MixedGroups);
    }

    [Test]
    public async Task Add_Edge_Between_Group_Root_Members_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        var result = g.AddEdge(A, B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.IllegalTree);
    }

    [Test]
    public async Task Add_Edge_Between_Group_Root_Member_Subtrees_Returns_Error()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        var result = g.AddEdge(B, C);

        await Assert.That(result).IsEqualTo(CongaModificationResult.IllegalTree);
    }

    [Test]
    public async Task Remove_Member_From_Group_Removes_Member_From_Group()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);

        var result = g.RemoveGroupMember(Group1, A);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(g.Children).Contains(n => n.Name == Group1);
        await Assert.That(g.Nodes).DoesNotContain(n => n.Name == A);
        await Assert.That(g.Children).DoesNotContain(n => n.Name == A);
        await Assert.That(g.Nodes).DoesNotContain(n => n.Name == B);
        await Assert.That(g.Children).DoesNotContain(n => n.Name == B);

        var group = g.Nodes.Single(n => n.Name == Group1) as CongaNode.GroupNode;

        await Assert.That(group).IsNotNull();
        await Assert.That(group.Children).DoesNotContain(n => n.Name == A);
        await Assert.That(group.Children).DoesNotContain(n => n.Name == B);
    }

    [Test]
    public async Task Task_Node_Is_Complete_When_Task_Does_Not_Exist()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var a = g.Nodes.Single(n => n.Name == A);

        var result = a.IsComplete([]);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Task_Node_Is_Complete_When_Task_Is_Complete()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var aTask = CreateTask(A, true);

        var a = g.Nodes.Single(n => n.Name == A);

        var result = a.IsComplete([aTask]);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Task_Node_Is_Incomplete_When_Task_Is_Incomplete()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var aTask = CreateTask(A, false);

        var a = g.Nodes.Single(n => n.Name == A);

        var result = a.IsComplete([aTask]);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Group_Node_Is_Complete_When_All_Inner_Trees_Are_Complete()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);
        g.AddGroupMember(Group1, C);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, true);
        var cTask = CreateTask(C, true);

        var group = g.Nodes.Single(n => n.Name == Group1);

        var result = group.IsComplete([aTask, bTask, cTask]);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Group_Node_Is_Incomplete_When_All_Inner_Trees_Are_Not_Complete()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);
        g.AddGroupMember(Group1, C);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);

        var group = g.Nodes.Single(n => n.Name == Group1);

        var result = group.IsComplete([aTask, bTask, cTask]);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Node_With_No_Parents_Does_Not_Activate()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);

        var a = g.Nodes.Single(n => n.Name == A);

        await Assert.That(a.CanBeActivated([aTask, bTask])).IsFalse();
    }

    [Test]
    public async Task Completed_Node_Does_Not_Activate()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, true);

        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(b.CanBeActivated([aTask, bTask])).IsFalse();
    }

    [Test]
    public async Task Node_With_One_Parent_Activates_When_Parent_Completes()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);

        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(b.CanBeActivated([aTask, bTask])).IsTrue();
    }

    [Test]
    public async Task Node_With_Multiple_Parents_Does_Not_Activate_When_Not_All_Parents_Are_Completed()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(C, B);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, false);

        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(b.CanBeActivated([aTask, bTask, cTask])).IsFalse();
    }

    [Test]
    public async Task Node_With_Multiple_Parents_Activates_When_All_Parents_Are_Completed()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(C, B);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);

        var b = g.Nodes.Single(n => n.Name == B);

        await Assert.That(b.CanBeActivated([aTask, bTask, cTask])).IsTrue();
    }

    [Test]
    public async Task Group_Activates_Root_Nodes()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddGroupMember(Group1, E);
        g.AddEdge(A, C);
        g.AddEdge(D, Group1);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, true);
        var eTask = CreateTask(E, false);

        var a = g.Nodes.Single(n => n.Name == A);
        var b = g.Nodes.Single(n => n.Name == B);
        var c = g.Nodes.Single(n => n.Name == C);
        var d = g.Nodes.Single(n => n.Name == D);
        var e = g.Nodes.Single(n => n.Name == E);
        var result = d.GetActivatedNodes([aTask, bTask, cTask, dTask, eTask]);

        await Assert.That(result).Contains(b);
        await Assert.That(result).Contains(e);
        await Assert.That(result).DoesNotContain(a);
        await Assert.That(result).DoesNotContain(c);
        await Assert.That(result).DoesNotContain(d);
    }

    [Test]
    public async Task Node_In_Group_Subtree_Activates_Dependents_When_Subtree_Is_Not_Completed()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, false);

        var b = g.Nodes.Single(n => n.Name == B);
        var c = g.Nodes.Single(n => n.Name == C);
        var d = g.Nodes.Single(n => n.Name == D);
        var result = c.GetActivatedNodes([aTask, bTask, cTask, dTask]);

        await Assert.That(result).Contains(d);
        await Assert.That(result).DoesNotContain(b);
    }

    [Test]
    public async Task Node_In_Group_Subtree_Activates_Group_Roots_When_Subtree_Is_Completed()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, true);

        var b = g.Nodes.Single(n => n.Name == B);
        var c = g.Nodes.Single(n => n.Name == C);
        var d = g.Nodes.Single(n => n.Name == D);
        var result = c.GetActivatedNodes([aTask, bTask, cTask, dTask]);

        await Assert.That(result).Contains(b);
        await Assert.That(result).DoesNotContain(d);
    }

    [Test]
    public async Task Node_In_Group_Subtree_Activates_Nothing_When_Subtree_Is_Not_Completed_And_Node_Has_No_Dependents()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);
        g.AddEdge(C, E);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, false);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, false);
        var eTask = CreateTask(E, true);

        var e = g.Nodes.Single(n => n.Name == E);
        var result = e.GetActivatedNodes([aTask, bTask, cTask, dTask, eTask]);

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Node_In_Group_Does_Not_Activate_Other_Nodes_Incomplete_Subtree()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);
        g.AddEdge(C, E);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, true);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, false);
        var eTask = CreateTask(E, true);

        var b = g.Nodes.Single(n => n.Name == B);
        var result = b.GetActivatedNodes([aTask, bTask, cTask, dTask, eTask]);

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task Node_In_Group_Activates_Downstream_Nodes_When_Group_Is_Complete()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);
        g.AddEdge(Group1, E);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, true);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, true);
        var eTask = CreateTask(E, false);

        var b = g.Nodes.Single(n => n.Name == B);
        var e = g.Nodes.Single(n => n.Name == E);
        var result = b.GetActivatedNodes([aTask, bTask, cTask, dTask, eTask]);

        await Assert.That(result).Contains(e);
    }

    [Test]
    public async Task Node_In_Group_Subtree_Activates_Downstream_Nodes_When_Group_Is_Complete()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, C);
        g.AddEdge(C, D);
        g.AddEdge(Group1, E);

        var aTask = CreateTask(A, true);
        var bTask = CreateTask(B, true);
        var cTask = CreateTask(C, true);
        var dTask = CreateTask(D, true);
        var eTask = CreateTask(E, false);

        var c = g.Nodes.Single(n => n.Name == C);
        var e = g.Nodes.Single(n => n.Name == E);
        var result = c.GetActivatedNodes([aTask, bTask, cTask, dTask, eTask]);

        await Assert.That(result).Contains(e);
    }

    [Test]
    public async Task Remove_Edge_Removes_Nodes_With_No_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        var result = g.RemoveEdge(A, B);

        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(a).IsNull();
        await Assert.That(b).IsNull();
    }

    [Test]
    public async Task Remove_Edge_Does_Not_Remove_Nodes_With_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddEdge(A, B);
        g.AddEdge(B, C);
        var result = g.RemoveEdge(A, B);

        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);
        var c = g.Nodes.SingleOrDefault(n => n.Name == C);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(a).IsNull();
        await Assert.That(b).IsNotNull();
        await Assert.That(c).IsNotNull();
    }

    [Test]
    public async Task Remove_Edge_Inside_Group_Removes_Nodes_With_No_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);
        g.AddEdge(B, C);
        var result = g.RemoveEdge(A, B);

        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);
        var c = g.Nodes.SingleOrDefault(n => n.Name == C);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(a).IsNull();
        await Assert.That(b).IsNotNull();
        await Assert.That(c).IsNotNull();
    }

    [Test]
    public async Task Remove_Edge_Inside_Group_Does_Not_Remove_Nodes_With_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(A, B);
        g.AddEdge(B, C);
        var result = g.RemoveEdge(A, B);

        var group = g.Nodes.SingleOrDefault(n => n.Name == Group1);
        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);
        var c = g.Nodes.SingleOrDefault(n => n.Name == C);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(group).IsNotNull();
        await Assert.That(a).IsNull();
        await Assert.That(b).IsNotNull();
        await Assert.That(c).IsNotNull();
    }

    [Test]
    public async Task Group_With_No_Remaining_Links_Is_Not_Removed()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, A);
        g.AddEdge(B, Group1);
        var result = g.RemoveEdge(B, Group1);

        var group = g.Nodes.SingleOrDefault(n => n.Name == Group1);
        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(group).IsNotNull();
        await Assert.That(a).IsNotNull();
        await Assert.That(b).IsNull();
    }

    [Test]
    public async Task Remove_Group_Removes_Nodes_With_No_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, Group1);
        var result = g.RemoveGroup(Group1);

        var group = g.Nodes.SingleOrDefault(n => n.Name == Group1);
        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(group).IsNull();
        await Assert.That(a).IsNull();
        await Assert.That(b).IsNull();
    }

    [Test]
    public async Task Remove_Group_Does_Not_Remove_Nodes_With_Remaining_Links()
    {
        var g = new CongaGraph();

        g.AddGroup(Group1);
        g.AddGroupMember(Group1, B);
        g.AddEdge(A, Group1);
        g.AddEdge(C, A);
        var result = g.RemoveGroup(Group1);

        var group = g.Nodes.SingleOrDefault(n => n.Name == Group1);
        var a = g.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g.Nodes.SingleOrDefault(n => n.Name == B);
        var c = g.Nodes.SingleOrDefault(n => n.Name == C);

        await Assert.That(result).IsEqualTo(CongaModificationResult.Success);
        await Assert.That(group).IsNull();
        await Assert.That(a).IsNotNull();
        await Assert.That(b).IsNull();
        await Assert.That(c).IsNotNull();
    }

    [Test]
    public async Task Graph_Dto_Roundtrip_Is_Successful()
    {
        var g = new CongaGraph();
        g.AddGroup(Group1);

        g.AddGroupMember(Group1, B);
        g.AddGroupMember(Group1, D);
        g.AddEdge(B, C);

        g.AddEdge(A, Group1);
        g.AddEdge(Group1, E);

        var dto = g.ToDto();

        await Assert.That(dto.Groups).Contains(n => n.Name == Group1);
        await Assert.That(dto.Edges.Count).IsEqualTo(2);

        var groupDto = dto.Groups.SingleOrDefault(n => n.Name == Group1);
        await Assert.That(groupDto).IsNotNull();
        await Assert.That(groupDto.Edges.Count).IsEqualTo(2);

        var g2 = CongaGraph.FromDto(dto);

        var group = g2.Nodes.SingleOrDefault(n => n.Name == Group1) as CongaNode.GroupNode;
        var a = g2.Nodes.SingleOrDefault(n => n.Name == A);
        var b = g2.Nodes.SingleOrDefault(n => n.Name == B);
        var c = g2.Nodes.SingleOrDefault(n => n.Name == C);
        var d = g2.Nodes.SingleOrDefault(n => n.Name == D);
        var e = g2.Nodes.SingleOrDefault(n => n.Name == E);

        await Assert.That(group).IsNotNull();
        await Assert.That(a).IsNotNull();
        await Assert.That(b).IsNotNull();
        await Assert.That(c).IsNotNull();
        await Assert.That(d).IsNotNull();
        await Assert.That(e).IsNotNull();

        await Assert.That(g2.Children).Contains(group);
        await Assert.That(g2.Children).Contains(a);
        await Assert.That(g2.Children).Contains(e);

        await Assert.That(a.Dependents).Contains(group);
        await Assert.That(group.Prerequisites).Contains(a);
        await Assert.That(group.Dependents).Contains(e);
        await Assert.That(e.Prerequisites).Contains(group);

        await Assert.That(group.Children).Contains(b);
        await Assert.That(group.Children).Contains(c);
        await Assert.That(group.Children).Contains(d);

        await Assert.That(b.Dependents).Contains(c);
        await Assert.That(c.Prerequisites).Contains(b);
    }

    private static Domain.Entities.Task CreateTask(
        Abbreviation abbreviation,
        bool isDone,
        bool isPseudo = false
    )
    {
        return new Domain.Entities.Task
        {
            ProjectId = ProjectId.FromNewGuid(),
            EpisodeId = EpisodeId.FromNewGuid(),
            AssigneeId = UserId.FromNewGuid(),
            Abbreviation = abbreviation,
            Name = abbreviation.Value,
            Weight = 0,
            IsPseudo = isPseudo,
            IsDone = isDone,
        };
    }
}
