   M S B A M L   `   `   `  ����+  'PredefinedControlAndInsertionAppProject>    8PredefinedControlAndInsertionAppProject.LoadConfigDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Load Configuration��5   	   $	��600��6$   $	��980��61    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ 	CanResize=�6-    � ShowInTaskbar$ False��6D   .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   �� 5
      $
��Auto �6   .+�5   
   �� $��0��6   $%��Select a configuration to load:��6!   $��0,0,0,10q�6H   �� 5   
   -   5        $��lbConfigurations��5      $��1��6   $��0,0,0,10q�67   5      �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5        $��	BtnDelete��5      $��Delete��6&   $��80��67   $��30��6B   $��0,0,10,0q�6N   ${�False��5   -   �� 5      -   5        $��BtnLoad��5      $
��Load��6$   $��80��63   $��30��6>   $��0,0,10,0q�6J    �� 	IsDefault$
 True��5   +   ${�False��6<   �� 5      $��Cancel��6   $��80��6&   $��30��61    �� IsCancel$
 True��6=   5      5      5      