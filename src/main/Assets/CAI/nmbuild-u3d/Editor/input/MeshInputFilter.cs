/*
 * Copyright (c) 2012 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System.Collections.Generic;
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d;
using UnityEngine;

/// <summary>
/// Filters out all MeshFilter components that reference a mesh in the list.
/// </summary>
[System.Serializable]
public class MeshInputFilter 
    : InputBuildProcessor
{
    /// <summary>
    /// The list of meshes to filter out.
    /// </summary>
    public List<Mesh> meshes = new List<Mesh>();

    /// <summary>
    /// The type of match to use when filtering.
    /// </summary>
    public MatchType matchType;

    [SerializeField]
    private int mPriority = NMBuild.DefaultPriority;

    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public override int Priority { get { return mPriority; } }

    /// <summary>
    /// Multiple processors of this type are allowed. (Always true.)
    /// </summary>
    public override bool DuplicatesAllowed { get { return true; } }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    /// <param name="value">The new priority.</param>
    public void SetPriority(int value)
    {
        mPriority = NMBuild.ClampPriority(value);
    }

    /// <summary>
    /// Processes the context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied during the <see cref="InputBuildState.FilterComponents"/> state.
    /// </para>
    /// </remarks>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public override bool ProcessInput(InputBuildState state, InputBuildContext context)
    {
        if (state != InputBuildState.FilterComponents || context == null)
            return true;

        if (meshes == null)
        {
            context.LogError(name + " Mesh exclusion list is null. (Invalid processor state.)"
                , this);
            return false;
        }

        if (meshes.Count == 0)
            // Nothing to do.
            return true;

        List<Component> targetFilters = context.components;

        int removed = 0;
        for (int iTarget = targetFilters.Count - 1; iTarget >= 0; iTarget--)
        {
            if (!(targetFilters[iTarget] is MeshFilter))
                continue;

            MeshFilter filter = (MeshFilter)targetFilters[iTarget];

            if (filter == null)
                continue;

            MatchPredicate p = new MatchPredicate(filter.sharedMesh, matchType);

            int iSource = meshes.FindIndex(p.Matches);

            if (iSource != -1)
            {
                targetFilters.RemoveAt(iTarget);
                removed++;
            }
        }

        if (removed > 0)
        {
            context.Log(string.Format("{0}: Filtered out {1} components.", name, removed), this);
        }
        else
            context.Log(name + ": No components filtered.", this);

        return true;
    }
}