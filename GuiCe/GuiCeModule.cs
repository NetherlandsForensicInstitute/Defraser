/*
 * Copyright (c) 2006-2020, Netherlands Forensic Institute
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the Netherlands Forensic Institute nor the names 
 *    of its contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autofac;
using Autofac.Builder;
using Defraser.FFmpegConverter;
using Defraser.Framework;
using Defraser.GuiCe.sendtolist;
using Defraser.Interface;
using Defraser.Util;
using WeifenLuo.WinFormsUI.Docking;

namespace Defraser.GuiCe
{
	/// <summary>
	/// The <see cref="IModule"/> containing the components of the Defraser GUI.
	/// </summary>
	/// <see cref="http://code.google.com/p/autofac/wiki/StructuringWithModules"/>
	public class GuiCeModule : Module
	{
		#region Inner classes
		/// <summary>
		/// The <see cref="IFormFactory"/> that supports any <see cref="Form"/>
		/// registered with the <see cref="GuiCeModule"/>.
		/// </summary>
		private sealed class FormFactory : IFormFactory
		{
			private readonly Creator<Form, Type> _createForm;

			/// <summary>
			/// Creates a new <see cref="FormFactory"/>.
			/// </summary>
			/// <param name="createForm">The factory method for creating forms</param>
			public FormFactory(Creator<Form, Type> createForm)
			{
				_createForm = createForm;
			}

			public T Create<T>() where T : Form
			{
				return (T)_createForm(typeof(T));
			}
		}

		/// <summary>
		/// The <see cref="IndirectSynchronizeInvoke"/> solves a circular dependency
		/// between the <see cref="BackgroundFileScanner"/> and the <see cref="MainForm"/>.
		/// </summary>
		private sealed class IndirectSynchronizeInvoke : ISynchronizeInvoke
		{
			private readonly Func<ISynchronizeInvoke> _synchronizeInvokeProvider;

			#region Properties
			public bool InvokeRequired { get { return SynchronizeInvoke.InvokeRequired; } }
			private ISynchronizeInvoke SynchronizeInvoke { get { return _synchronizeInvokeProvider(); } }
			#endregion Properties

			/// <summary>
			/// Creates a new <see cref="IndirectSynchronizeInvoke"/>.
			/// </summary>
			/// <param name="synchronizeInvokeProvider">
			/// The function that supplies the actual <see cref="ISynchronizeInvoke"/>
			/// when it is used. In our case, this is the <see cref="MainForm"/>.
			/// </param>
			internal IndirectSynchronizeInvoke(Func<ISynchronizeInvoke> synchronizeInvokeProvider)
			{
				_synchronizeInvokeProvider = synchronizeInvokeProvider;
			}

			public IAsyncResult BeginInvoke(Delegate method, object[] args)
			{
				return SynchronizeInvoke.BeginInvoke(method, args);
			}

			public object EndInvoke(IAsyncResult result)
			{
				return SynchronizeInvoke.EndInvoke(result);
			}

			public object Invoke(Delegate method, object[] args)
			{
				return SynchronizeInvoke.Invoke(method, args);
			}
		}
		#endregion Inner classes

		#region Properties
		private static Parameter CurrentProjectParameter
		{
			get
			{
				return new ResolvedTypedParameter(typeof(IProject), c => c.Resolve<ProjectManager>().Project);
			}
		}
		private static Parameter CurrentProjectMetadataParameter
		{
			get
			{
				return new ResolvedTypedParameter(typeof(IDictionary<ProjectMetadataKey, string>), c => c.Resolve<ProjectManager>().Project.GetMetadata());
			}
		}
		private static Parameter EditModeParameter
		{
			get
			{
				return new ResolvedTypedParameter(typeof(bool), c => c.Resolve<ProjectManager>().Project.GetInputFiles().Count == 0);
			}
		}
		#endregion Properties

		protected override void Load(ContainerBuilder builder)
        {
            #region Register main form and common components
            builder.Register<BackgroundDataBlockScanner>().FactoryScoped();
			builder.Register<BackgroundFileScanner>();
			builder.Register<FullFileScanner>();
			builder.Register<DockPanel>();
			builder.Register<FileTreeObject>();
			builder.Register<FormFactory>().As<IFormFactory>();
			builder.Register(c => new IndirectSynchronizeInvoke(() => c.Resolve<MainForm>())).As<ISynchronizeInvoke>();
			builder.Register<HeaderPanel>().FactoryScoped();
			builder.Register<HeaderTree>().FactoryScoped();
			builder.Register<HeaderDetailTree>();
			builder.Register<MainForm>();
			builder.Register<SendToList>().SingletonScoped();
			builder.Register<SendToItemFactory>();
			builder.Register<WorkpadManager>();
			builder.Register<FramePreviewManager>();
            builder.Register<VideoKeyframesManager>();
		    builder.Register<DefaultCodecHeaderManager>().As<DefaultCodecHeaderManager>().As<ICodecHeaderSource>();
            builder.Register<ParametersChecker>();
			#endregion Register main form and common components

			#region Register forms
			builder.Register<AboutBox>().FactoryScoped();
			builder.Register<AddFile>().WithArguments(CurrentProjectParameter).FactoryScoped();
			builder.Register<AddSendToItemForm>().FactoryScoped();
			builder.Register<AdvancedDetectorConfiguration>().WithArguments(EditModeParameter).FactoryScoped();
			builder.Register<ColumnChooser>().FactoryScoped();
			builder.Register<CreateNewProjectForm>().FactoryScoped();
			builder.Register<EditSendToItemForm>().FactoryScoped();
			builder.Register<OpenOrCreateProject>().FactoryScoped();
			builder.Register<Options>().FactoryScoped();
			builder.Register<SearchHeader>().FactoryScoped();
            builder.Register<SelectingKeyframe>().FactoryScoped();
			builder.Register<ProjectProperties>().WithArguments(CurrentProjectMetadataParameter).FactoryScoped();
			builder.Register<RenameForm>().FactoryScoped();
			builder.Register<SendToForm>().FactoryScoped();
			builder.Register<SendToProgressReporter>().FactoryScoped();
            builder.Register<VideoKeyframesWindow>().FactoryScoped();
            builder.Register<EditDefaultCodecHeader>().FactoryScoped();
			builder.Register<ProjectKeyframeOverview>().FactoryScoped();
			builder.Register<Workpad>().FactoryScoped();
			// Factory method for creating forms
            // Generic creator for forms.
			builder.Register<Creator<Form, Type>>(c => (t => (Form)c.Resolve(t)));
            // Specific creator for workpad rename.
		    builder.Register<Creator<RenameForm, Workpad>>(c => (w => new RenameForm(w)));
			#endregion Register forms

			#region Register generated factories
			builder.RegisterCreator<IProgressReporter, BackgroundWorker>();
		    builder.RegisterCreator<VideoKeyframesWindow>();
            builder.RegisterCreator<EditDefaultCodecHeader>();
			builder.RegisterCreator<SearchHeader>();
            builder.RegisterCreator<SelectingKeyframe>();
			builder.RegisterCreator<Workpad>();
			#endregion Register generated factories
		}
	}
}
