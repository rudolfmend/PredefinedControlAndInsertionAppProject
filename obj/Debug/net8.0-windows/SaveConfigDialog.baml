   M S B A M L   `   `   `  ����+  'PredefinedControlAndInsertionAppProject>    8PredefinedControlAndInsertionAppProject.SaveConfigDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Save Configuration��5   	   $	��400��6$   $	��600��61    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ NoResize=�6-    � ShowInTaskbar$ False��6C   .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $)��#Save your automation configuration:��6!   $��0,0,0,10q�6L   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��* �6#   .+�5      �� $��0��6   $��Configuration name:��6(   $��Center=�6C   $��0,0,10,0q�6^   �� 5      -   5        $��txtConfigName��5   &   $��1��6   $��Center=�6;   $	��5,3q�6]   5      �� 5   
   -   5        $��
txtWarning��5   !   $��2��6   $
��Wrap=�63   
���  ��$y�	Collapsed=�5      $��0,10,0,0q�6+   �� 5   
   $��3��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�5      �� -   5        $
��Save��5      $��80��6$   $��30��6/    �� 	IsDefault$
 True��6;   $��0,0,10,0q�6L   6^   �� 5      $��Cancel��6   $��80��6&   $��30��61    �� IsCancel$
 True��6=   5       5!      5"      