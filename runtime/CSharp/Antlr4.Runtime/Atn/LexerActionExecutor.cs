// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* [The "BSD license"]
*  Copyright (c) 2013 Terence Parr
*  Copyright (c) 2013 Sam Harwell
*  All rights reserved.
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions
*  are met:
*
*  1. Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*  2. Redistributions in binary form must reproduce the above copyright
*     notice, this list of conditions and the following disclaimer in the
*     documentation and/or other materials provided with the distribution.
*  3. The name of the author may not be used to endorse or promote products
*     derived from this software without specific prior written permission.
*
*  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
*  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
*  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
*  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
*  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
*  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
*  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// Represents an executor for a sequence of lexer actions which traversed during
    /// the matching operation of a lexer rule (token).
    /// </summary>
    /// <remarks>
    /// Represents an executor for a sequence of lexer actions which traversed during
    /// the matching operation of a lexer rule (token).
    /// <p>The executor tracks position information for position-dependent lexer actions
    /// efficiently, ensuring that actions appearing only at the end of the rule do
    /// not cause bloating of the
    /// <see cref="Antlr4.Runtime.Dfa.DFA"/>
    /// created for the lexer.</p>
    /// </remarks>
    /// <author>Sam Harwell</author>
    /// <since>4.2</since>
    public class LexerActionExecutor
    {
        [NotNull]
        private readonly ILexerAction[] lexerActions;

        /// <summary>
        /// Caches the result of
        /// <see cref="hashCode"/>
        /// since the hash code is an element
        /// of the performance-critical
        /// <see cref="LexerATNConfig#hashCode"/>
        /// operation.
        /// </summary>
        private readonly int hashCode;

        /// <summary>
        /// Constructs an executor for a sequence of
        /// <see cref="ILexerAction"/>
        /// actions.
        /// </summary>
        /// <param name="lexerActions">The lexer actions to execute.</param>
        public LexerActionExecutor(ILexerAction[] lexerActions)
        {
            this.lexerActions = lexerActions;
            int hash = MurmurHash.Initialize();
            foreach (ILexerAction lexerAction in lexerActions)
            {
                hash = MurmurHash.Update(hash, lexerAction);
            }
            this.hashCode = MurmurHash.Finish(hash, lexerActions.Length);
        }

        /// <summary>
        /// Creates a
        /// <see cref="LexerActionExecutor"/>
        /// which executes the actions for
        /// the input
        /// <paramref name="lexerActionExecutor"/>
        /// followed by a specified
        /// <paramref name="lexerAction"/>
        /// .
        /// </summary>
        /// <param name="lexerActionExecutor">
        /// The executor for actions already traversed by
        /// the lexer while matching a token within a particular
        /// <see cref="ATNConfig"/>
        /// . If this is
        /// <see langword="null"/>
        /// , the method behaves as though
        /// it were an empty executor.
        /// </param>
        /// <param name="lexerAction">
        /// The lexer action to execute after the actions
        /// specified in
        /// <paramref name="lexerActionExecutor"/>
        /// .
        /// </param>
        /// <returns>
        /// A
        /// <see cref="LexerActionExecutor"/>
        /// for executing the combine actions
        /// of
        /// <paramref name="lexerActionExecutor"/>
        /// and
        /// <paramref name="lexerAction"/>
        /// .
        /// </returns>
        [NotNull]
        public static Antlr4.Runtime.Atn.LexerActionExecutor Append(Antlr4.Runtime.Atn.LexerActionExecutor lexerActionExecutor, ILexerAction lexerAction)
        {
            if (lexerActionExecutor == null)
            {
                return new Antlr4.Runtime.Atn.LexerActionExecutor(new ILexerAction[] { lexerAction });
            }
            ILexerAction[] lexerActions = Arrays.CopyOf(lexerActionExecutor.lexerActions, lexerActionExecutor.lexerActions.Length + 1);
            lexerActions[lexerActions.Length - 1] = lexerAction;
            return new Antlr4.Runtime.Atn.LexerActionExecutor(lexerActions);
        }

        /// <summary>
        /// Creates a
        /// <see cref="LexerActionExecutor"/>
        /// which encodes the current offset
        /// for position-dependent lexer actions.
        /// <p>Normally, when the executor encounters lexer actions where
        /// <see cref="ILexerAction.IsPositionDependent()"/>
        /// returns
        /// <see langword="true"/>
        /// , it calls
        /// <see cref="Antlr4.Runtime.IIntStream.Seek(int)"/>
        /// on the input
        /// <see cref="Antlr4.Runtime.ICharStream"/>
        /// to set the input
        /// position to the <em>end</em> of the current token. This behavior provides
        /// for efficient DFA representation of lexer actions which appear at the end
        /// of a lexer rule, even when the lexer rule matches a variable number of
        /// characters.</p>
        /// <p>Prior to traversing a match transition in the ATN, the current offset
        /// from the token start index is assigned to all position-dependent lexer
        /// actions which have not already been assigned a fixed offset. By storing
        /// the offsets relative to the token start index, the DFA representation of
        /// lexer actions which appear in the middle of tokens remains efficient due
        /// to sharing among tokens of the same length, regardless of their absolute
        /// position in the input stream.</p>
        /// <p>If the current executor already has offsets assigned to all
        /// position-dependent lexer actions, the method returns
        /// <c>this</c>
        /// .</p>
        /// </summary>
        /// <param name="offset">
        /// The current offset to assign to all position-dependent
        /// lexer actions which do not already have offsets assigned.
        /// </param>
        /// <returns>
        /// A
        /// <see cref="LexerActionExecutor"/>
        /// which stores input stream offsets
        /// for all position-dependent lexer actions.
        /// </returns>
        public virtual Antlr4.Runtime.Atn.LexerActionExecutor FixOffsetBeforeMatch(int offset)
        {
            ILexerAction[] updatedLexerActions = null;
            for (int i = 0; i < lexerActions.Length; i++)
            {
                if (lexerActions[i].IsPositionDependent && !(lexerActions[i] is LexerIndexedCustomAction))
                {
                    if (updatedLexerActions == null)
                    {
                        updatedLexerActions = lexerActions.Clone();
                    }
                    updatedLexerActions[i] = new LexerIndexedCustomAction(offset, lexerActions[i]);
                }
            }
            if (updatedLexerActions == null)
            {
                return this;
            }
            return new Antlr4.Runtime.Atn.LexerActionExecutor(updatedLexerActions);
        }

        /// <summary>Gets the lexer actions to be executed by this executor.</summary>
        /// <remarks>Gets the lexer actions to be executed by this executor.</remarks>
        /// <returns>The lexer actions to be executed by this executor.</returns>
        public virtual ILexerAction[] LexerActions
        {
            get
            {
                return lexerActions;
            }
        }

        /// <summary>
        /// Execute the actions encapsulated by this executor within the context of a
        /// particular
        /// <see cref="Antlr4.Runtime.Lexer"/>
        /// .
        /// <p>This method calls
        /// <see cref="Antlr4.Runtime.IIntStream.Seek(int)"/>
        /// to set the position of the
        /// <paramref name="input"/>
        /// 
        /// <see cref="Antlr4.Runtime.ICharStream"/>
        /// prior to calling
        /// <see cref="ILexerAction.Execute(Antlr4.Runtime.Lexer)"/>
        /// on a position-dependent action. Before the
        /// method returns, the input position will be restored to the same position
        /// it was in when the method was invoked.</p>
        /// </summary>
        /// <param name="lexer">The lexer instance.</param>
        /// <param name="input">
        /// The input stream which is the source for the current token.
        /// When this method is called, the current
        /// <see cref="Antlr4.Runtime.IIntStream.Index()"/>
        /// for
        /// <paramref name="input"/>
        /// should be the start of the following token, i.e. 1
        /// character past the end of the current token.
        /// </param>
        /// <param name="startIndex">
        /// The token start index. This value may be passed to
        /// <see cref="Antlr4.Runtime.IIntStream.Seek(int)"/>
        /// to set the
        /// <paramref name="input"/>
        /// position to the beginning
        /// of the token.
        /// </param>
        public virtual void Execute(Lexer lexer, ICharStream input, int startIndex)
        {
            bool requiresSeek = false;
            int stopIndex = input.Index;
            try
            {
                foreach (ILexerAction lexerAction in lexerActions)
                {
                    if (lexerAction is LexerIndexedCustomAction)
                    {
                        int offset = ((LexerIndexedCustomAction)lexerAction).Offset;
                        input.Seek(startIndex + offset);
                        lexerAction = ((LexerIndexedCustomAction)lexerAction).Action;
                        requiresSeek = (startIndex + offset) != stopIndex;
                    }
                    else
                    {
                        if (lexerAction.IsPositionDependent)
                        {
                            input.Seek(stopIndex);
                            requiresSeek = false;
                        }
                    }
                    lexerAction.Execute(lexer);
                }
            }
            finally
            {
                if (requiresSeek)
                {
                    input.Seek(stopIndex);
                }
            }
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            else
            {
                if (!(obj is Antlr4.Runtime.Atn.LexerActionExecutor))
                {
                    return false;
                }
            }
            Antlr4.Runtime.Atn.LexerActionExecutor other = (Antlr4.Runtime.Atn.LexerActionExecutor)obj;
            return hashCode == other.hashCode && Arrays.Equals(lexerActions, other.lexerActions);
        }
    }
}
