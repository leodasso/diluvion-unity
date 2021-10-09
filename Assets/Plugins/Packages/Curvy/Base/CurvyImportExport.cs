// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class to convert between Curvy and JSON
    /// </summary>
    /// <remarks>Inherit from this and override the handlers to add custom functionality</remarks>
    public class CurvyImportExport : MonoBehaviour
    {
        public enum ExportOptions
        {
            Splines,
            ControlPoints
        }

        public enum ImportOptions
        {
            Create,
            Apply,
            Insert
        }

        [Section("General")]
        [FieldCondition("FilePath", "", false, ActionAttribute.ActionEnum.ShowWarning, "Missing File Path")]
        [PathSelectorAttribute(PathSelectorAttribute.DialogMode.CreateFile, Title ="Select File")]
        public string FilePath;
        public CurvySerializationSpace Space = CurvySerializationSpace.WorldSpline;

        [Section("Import")]
        
        public ImportOptions Mode = ImportOptions.Create;
        [FieldCondition("Mode",ImportOptions.Create)]
        public Transform Target;
        [FieldCondition("Mode", ImportOptions.Apply)]
        [ArrayEx]
        public CurvySpline[] ApplyTo;
        [FieldCondition("Mode", ImportOptions.Insert)]
        public CurvySplineSegment InsertAfter;
        [FieldAction("ShowImportButton")]
        public CurvySplineEvent OnDeserializedSpline;
        public CurvyControlPointEvent OnDeserializedCP;

        [Section("Export")]
        public ExportOptions ExportOption = ExportOptions.Splines;
        [FieldCondition("ExportOption",ExportOptions.Splines)]
        [FieldAction("ShowExportButton", ShowBelowProperty = true)]
        [ArrayEx()]
        public List<CurvySpline> SourceSplines;
        [FieldCondition("ExportOption", ExportOptions.ControlPoints)]
        [FieldAction("ShowExportButton", ShowBelowProperty = true)]
        [ArrayEx()]
        public List<CurvySplineSegment> SourceControlPoints;

       


        static Action<CurvySpline, string> mOnDeserializedSpline;
        static Action<CurvySplineSegment, string> mOnDeserializedCP;

        #region ### Public Methods & Properties ###

        public void Import()
        {
            if (OnDeserializedSpline.HasListeners())
                mOnDeserializedSpline = new Action<CurvySpline, string>((x, y) => { OnDeserializedSpline.Invoke(new CurvySplineEventArgs(this, x, y)); });
            else
                mOnDeserializedSpline = null;
            if (OnDeserializedCP.HasListeners())
                mOnDeserializedCP = new Action<CurvySplineSegment, string>((x, y) => { OnDeserializedCP.Invoke(new CurvyControlPointEventArgs(this,x.Spline, x,CurvyControlPointEventArgs.ModeEnum.Added, y)); });
            else
                mOnDeserializedCP = null;

            ImportFromFile(FilePath, Target, Space);
        }

        public void Export()
        {
        }

        

        #endregion

        #region ### Static import/export using strings ###

        public static void Deserialize(string json, Transform target, CurvySerializationSpace space)
        {
            if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineCollection))
            {
                var sspl = SerializedCurvySplineCollection.FromJson(json);
                sspl.Deserialize(target, space, mOnDeserializedSpline, mOnDeserializedCP);
            }
            else
                DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineCollection'!");
        }

        public static void Deserialize(string json, CurvySpline[] applyTo, CurvySerializationSpace space)
        {
            if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineCollection))
            {
                var sspl = SerializedCurvySplineCollection.FromJson(json);
                sspl.Deserialize(applyTo, space, mOnDeserializedSpline, mOnDeserializedCP);
            }
            else
                DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineCollection'!");
        }

        public static void Deserialize(string json, CurvySplineSegment insertAfter, CurvySerializationSpace space)
        {
            if (SerializedCurvyObjectHelper.GetJsonSerializedType(json) == typeof(SerializedCurvySplineSegmentCollection))
            {
                var sspl = SerializedCurvySplineSegmentCollection.FromJson(json);
                sspl.Deserialize(insertAfter, space, mOnDeserializedCP);
            }
            else
                DTLog.LogWarning("[Curvy] CurvyImportExport.Deserialize: Data isn't of type 'SerializedCurvySplineSegmentCollection'!");
        }


        public static string Serialize(CurvySerializationSpace space, params CurvySpline[] splines)
        {
            return new SerializedCurvySplineCollection(new List<CurvySpline>(splines), space).ToJson();
        }

        public static string Serialize(CurvySerializationSpace space, params CurvySplineSegment[] controlPoints)
        {
            return new SerializedCurvySplineSegmentCollection(new List<CurvySplineSegment>(controlPoints), space).ToJson();
        }

        #endregion

        #region ### Static import/export using files ###

        public static void ImportFromFile(string filePath, Transform target, CurvySerializationSpace space)
        {
            Deserialize(loadFile(filePath), target, space);
        }

        public static void ImportFromFile(string filePath, CurvySpline[] applyTo, CurvySerializationSpace space)
        {
            Deserialize(loadFile(filePath), applyTo, space);
        }

        public static void ImportFromFile(string filePath, CurvySplineSegment insertAfter, CurvySerializationSpace space)
        {
            Deserialize(loadFile(filePath), insertAfter, space);
        }

        public static void ExportToFile(string filePath, CurvySerializationSpace space, params CurvySpline[] splines)
        {
            saveFile(Serialize(space, splines),filePath);
        }

        public static void ExportToFile(string filePath, CurvySerializationSpace space, params CurvySplineSegment[] controlPoints)
        {
            saveFile(Serialize(space, controlPoints), filePath);
        }

        #endregion

        #region ### Static import/export using clipboard ###
        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        static void saveFile(string data, string filePath)
        {
            System.IO.File.WriteAllText(filePath, data);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
        }

        static string loadFile(string filePath)
        {
            return System.IO.File.ReadAllText(filePath).Replace("\n", "");
        }

        /*! \endcond */
        #endregion

    }




}
