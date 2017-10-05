﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Extension;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
{
  public static class ModifiersUtil
  {
    private const string AbstractClass = "AbstractClass";

    public static MemberDecoration GetDecoration(IUnionCaseDeclaration caseDeclaration)
    {
      if (caseDeclaration.FieldsEnumerable.IsEmpty())
        return MemberDecoration.FromModifiers(Modifiers.INTERNAL);

      var unionDeclaration = caseDeclaration.GetContainingTypeDeclaration() as IUnionDeclaration;
      return unionDeclaration != null
        ? GetDecoration(unionDeclaration.AccessModifiers, TreeNodeEnumerable<IFSharpAttribute>.Empty)
        : MemberDecoration.DefaultValue;
    }

    public static MemberDecoration GetDecoration(IAccessModifiers accessModifiers,
      TreeNodeEnumerable<IFSharpAttribute> attributes)
    {
      var decoration = MemberDecoration.DefaultValue;
      var modifiers = new JetHashSet<TokenNodeType>();

      foreach (var modifier in accessModifiers.Modifiers)
        modifiers.Add(modifier.GetTokenType());

      if (modifiers.Contains(FSharpTokenType.PUBLIC)) decoration.Modifiers |= Modifiers.PUBLIC;
      if (modifiers.Contains(FSharpTokenType.INTERNAL)) decoration.Modifiers |= Modifiers.INTERNAL;
      if (modifiers.Contains(FSharpTokenType.PRIVATE)) decoration.Modifiers |= Modifiers.PRIVATE;

      foreach (var attr in attributes)
      {
        var ids = attr.LongIdentifier.Identifiers;
        if (ids.IsEmpty) continue;

        var attributeShortName = ids.Last().GetText().GetAttributeShortName();
        if (attributeShortName.Equals(AbstractClass, StringComparison.Ordinal))
        {
          decoration.Modifiers |= Modifiers.ABSTRACT;
          break;
        }
      }
      return Normalize(decoration);
    }

    public static MemberDecoration GetModifiers(this TypePart typePart)
    {
        var sigPart = GetPartFromSignature(typePart);
        if (sigPart != null)
          return Normalize(sigPart.Modifiers);

        if (typePart == null)
          return MemberDecoration.DefaultValue;

        var decoration = typePart.Modifiers;
        var isHiddenBySignature = (typePart.GetRoot() as FSharpProjectFilePart)?.HasPairFile ?? false;
        if (isHiddenBySignature)
          decoration.AccessRights = AccessRights.INTERNAL;

        return Normalize(decoration);
    }

    private static MemberDecoration Normalize(MemberDecoration decoration)
    {
      if (decoration.AccessRights == AccessRights.NONE)
        decoration.AccessRights = AccessRights.PUBLIC;

      if (decoration.IsStatic)
      {
        decoration.IsAbstract = true;
        decoration.IsSealed = true;
      }

      return decoration;
    }

    [CanBeNull]
    private static TypePart GetPartFromSignature(TypePart typePart)
    {
      for (var part = typePart; part != null; part = part.NextPart)
      {
        var filePart = part.GetRoot() as FSharpProjectFilePart;
        if (filePart?.IsSignaturePart ?? false)
          return part;
      }
      return null;
    }
  }
}