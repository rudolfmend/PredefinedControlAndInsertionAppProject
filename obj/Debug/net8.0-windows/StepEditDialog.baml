   M S B A M L   `   `   `  ����+  'PredefinedControlAndInsertionAppProject<    6PredefinedControlAndInsertionAppProject.StepEditDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Edit Automation Step��5   	   $	��400��6&   $	��600��63    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ NoResize=�6-    � ShowInTaskbar$ False��6C   .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $&�� Edit automation step properties:��6!   $
��Bold"�6I   $��0,0,0,15q�6[   � 5   
   $��1��6   $��0,0,0,10q�6    � ColumnDefinitions 5      �� 5      $	��120 �6#   �� 5      $��* �6#   .+�5      �� $��0��6   $��Action:��6(   $��Center=�67   �� 5      -   5        $��	cmbAction��5   '   $��1��6    �� SelectedValuePath$ Content��68   .@�5      �� $��SetValue��6   �� 5      $��Click��6   �� 5      $
��Wait��6   5      5      � 5"   
   $��2��6   $��0,0,0,10q�6    5#      �� 5$      $	��120 �6#   �� 5%      $��* �6#   .+�5(      �� $��0��6   $��Target Element:��6(   $��Center=�6?   �� 5)      -   5        $��	cmbTarget��5)   '   $��1��6    �� DisplayMemberPath$
 Name��68   5*      � 5-   
   -   5        $��	valueGrid��5-   .   $��3��6   $��0,0,0,10q�6    5.      �� 5/      $	��120 �6#   �� 50      $��* �6#   .+�53      �� -   5        $��
valueLabel��53   Q   $��0��6   $��Value:��6(   $��Center=�66   6Q   �� 54      -   5        $��txtValue��54   &   $��1��6   6&   55      �� 58   
   -   5        $��stepDescription��58   !   $��4��6   $
��Wrap=�68   $��0,10,0,0q�6L   $2��,Explanation of this step will appear here...��59      �� 5<   
   $��5��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�5=      �� -   5        $��OK��5=      $��80��6"   $��30��6-   	 �� 	IsDefault$
	 True��69   $��0,0,10,0q�6J   6\   �� 5>      $��Cancel��6   $��80��6&   $��30��61   
 �� IsCancel$

 True��6=   5?      5@      5A      