   M S B A M L   `   `   `  ����+  'PredefinedControlAndInsertionAppProjectG    APredefinedControlAndInsertionAppProject.ApplicationSelectorDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Select Application��5   	   $	��600��6$   $	��980��61    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ 	CanResize=�5   	    � ShowInTaskbar$ False��6    .��5      � $��10q�6    � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $#��Select a running application:��6!   $��0,0,0,10q�6F   $��14(�6X   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $
��Auto �6#   �� 5      $��* �6#   .+�5      �� $��0��6   $��Applications��6'   .��5      �� -   5        $��lvApplications��5      $��5q�61    �� View 5      �� 5      .P�5      �� $��Application��6-    �� Width$	 150��6o    �� DisplayMemberBinding 6B   �� *ProcessName+6o   �� 5      $��Window Title��6-   $	 200��6t    6C   �� *MainWindowTitle+6t   5      5       5!      �� 5$      $��1��6   $��5��6+   $��Center=�65   $��Stretch=�6R   ���	#FFDDDDDD6n   �� 5&      $��2��6   $��Windows��6'   .��5'      �  5(      �� 5)      $��* �6(   �� 5*      $
��Auto �6(   .+�5-      �� -   5        $��	lvWindows��5-   ,   $��0��6   $��5q�6=    5.      �� 5/      .P�50   "   �� $��Window Title��61   $	 350��6t    6G   �� *WindowTitle+6t   51      53      �� 55      -   5        $��BtnShowElements��55   *   $��1��6   $��Show Elements��6A   $��5q�6Y   ${�False��56   ;   $��30��6M   57      58      59      �� 5;   
   $��2��6   	 �� Orientation$	 
Horizontal=�6"   $��Right=�6;   $��0,10,0,0q�6W   .+�5<      �� -   5        $��
BtnRefresh��5<      $��Refresh��6'   $��80��69   $��30��6D   $��0,0,10,0q�6P   5=      �� 5>      -   5        $��	BtnSelect��5>      $��Select��6&   $��80��67   $��30��6B   $��0,0,10,0q�6N   
 �� 	IsDefault$

 True��5?   -   ${�False��6>   �� 5@      -   5        $��	BtnCancel��5@      $��Cancel��6&   $��80��67   $��30��6B    �� IsCancel$
 True��5A   -   5B      5C      5D      